using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;
using Echo.DataFlow.Emulation;

namespace Echo.ControlFlow.Construction.Symbolic
{
    /// <summary>
    /// Provides an implementation of a control flow graph builder that traverses the instructions in a recursive manner,
    /// and maintains an symbolic program state to determine all possible branch targets of any indirect branching instruction.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the control flow graph.</typeparam>
    public class SymbolicFlowGraphBuilder<TInstruction> : FlowGraphBuilderBase<TInstruction>
    {
        /// <summary>
        /// Creates a new symbolic control flow graph builder using the provided program state transition resolver.  
        /// </summary>
        /// <param name="architecture">The architecture of the instructions.</param>
        /// <param name="instructions">The instructions to traverse.</param>
        /// <param name="transitionResolver">The transition resolver to use for inferring branch targets.</param>
        public SymbolicFlowGraphBuilder(
            IInstructionSetArchitecture<TInstruction> architecture,
            IEnumerable<TInstruction> instructions, 
            IStateTransitionResolver<TInstruction> transitionResolver)
        {
            Instructions = new StaticToSymbolicAdapter<TInstruction>(architecture, instructions);
            TransitionResolver = transitionResolver ?? throw new ArgumentNullException(nameof(transitionResolver));
        }
        
        /// <summary>
        /// Creates a new symbolic control flow graph builder using the provided program state transition resolver.  
        /// </summary>
        /// <param name="instructions">The instructions to traverse.</param>
        /// <param name="transitionResolver">The transition resolver to use for inferring branch targets.</param>
        public SymbolicFlowGraphBuilder(
            IStaticInstructionProvider<TInstruction> instructions, 
            IStateTransitionResolver<TInstruction> transitionResolver)
        {
            Instructions = new StaticToSymbolicAdapter<TInstruction>(
                instructions ?? throw new ArgumentNullException(nameof(instructions)));
            TransitionResolver = transitionResolver ?? throw new ArgumentNullException(nameof(transitionResolver));
        }
        
        /// <summary>
        /// Creates a new symbolic control flow graph builder using the provided program state transition resolver.  
        /// </summary>
        /// <param name="instructions">The instructions to traverse.</param>
        /// <param name="transitionResolver">The transition resolver to use for inferring branch targets.</param>
        public SymbolicFlowGraphBuilder(
            ISymbolicInstructionProvider<TInstruction> instructions, 
            IStateTransitionResolver<TInstruction> transitionResolver)
        {
            Instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
            TransitionResolver = transitionResolver ?? throw new ArgumentNullException(nameof(transitionResolver));
        }

        /// <summary>
        /// Gets the instructions to traverse.
        /// </summary>
        public ISymbolicInstructionProvider<TInstruction> Instructions
        {
            get;
        }

        /// <inheritdoc />
        public override IInstructionSetArchitecture<TInstruction> Architecture => Instructions.Architecture;

        /// <summary>
        /// Gets the object responsible for resolving every transition in the program state that an instruction might introduce. 
        /// </summary>
        public IStateTransitionResolver<TInstruction> TransitionResolver
        {
            get;
        }

        /// <inheritdoc />
        protected override IInstructionTraversalResult<TInstruction> CollectInstructions(
            long entrypoint, IEnumerable<long> knownBlockHeaders)
        {
            var context = new GraphBuilderContext(Architecture);
            var blockHeaders = knownBlockHeaders as long[] ?? knownBlockHeaders.ToArray();
            
            // Perform traversal.
            TraverseInstructions(context, entrypoint, blockHeaders);
            
            // Register known block headers.
            context.Result.BlockHeaders.Add(entrypoint);
            context.Result.BlockHeaders.UnionWith(blockHeaders);
            
            // Infer remaining block headers.
            DetermineBlockHeaders(context);
            
            return context.Result;
        }

        private void TraverseInstructions(in GraphBuilderContext context, long entrypoint, IEnumerable<long> knownBlockHeaders)
        {
            var agenda = new Stack<SymbolicProgramState<TInstruction>>();
            foreach (var header in knownBlockHeaders)
                agenda.Push(TransitionResolver.GetInitialState(header));
            agenda.Push(TransitionResolver.GetInitialState(entrypoint));

            while (agenda.Count > 0)
            {
                // Merge the current state with the recorded states.
                var currentState = agenda.Pop();
                bool recordedStatesChanged = ApplyStateChange(context, ref currentState);
                
                // If anything changed, we must invalidate the known successors of the current
                // instruction and (re)visit all its successors.
                if (recordedStatesChanged)
                {
                    var instruction = Instructions.GetCurrentInstruction(currentState);
                    
                    if (context.Result.ContainsInstruction(currentState.ProgramCounter))
                        context.Result.ClearSuccessors(instruction);
                    else
                        context.Result.AddInstruction(instruction);

                    ResolveAndScheduleSuccessors(context, currentState, instruction, agenda);
                }
            }
        }

        private static bool ApplyStateChange(
            GraphBuilderContext context, 
            ref SymbolicProgramState<TInstruction> currentState)
        {
            bool changed = false;
            if (context.RecordedStates.TryGetValue(currentState.ProgramCounter, out var recordedState))
            {
                // We are revisiting this address, merge program states.
                if (recordedState.MergeWith(currentState))
                {
                    currentState = recordedState;
                    changed = true;
                }
            }
            else
            {
                // This is a new unvisited address.
                context.RecordedStates.Add(currentState.ProgramCounter, currentState);
                changed = true;
            }

            return changed;
        }

        private void ResolveAndScheduleSuccessors(
            GraphBuilderContext context, 
            SymbolicProgramState<TInstruction> currentState,
            in TInstruction instruction,
            Stack<SymbolicProgramState<TInstruction>> agenda)
        {
            var result = context.Result;
            
            // Get a buffer to write to.
            var arrayPool = ArrayPool<StateTransition<TInstruction>>.Shared;
            int transitionCount = TransitionResolver.GetTransitionCount(currentState, instruction);
            var transitionsBuffer = arrayPool.Rent(transitionCount);

            try
            {
                // Read transitions.
                var transitionsBufferSlice = new Span<StateTransition<TInstruction>>(transitionsBuffer, 0, transitionCount);
                int actualTransitionCount = TransitionResolver.GetTransitions(currentState, instruction, transitionsBufferSlice);
                if (actualTransitionCount > transitionCount)
                    throw new InvalidOperationException();
                
                for (int i = 0; i < actualTransitionCount; i++)
                {
                    // Translate transition into successor info and register.
                    var transition = transitionsBufferSlice[i];
                    var successor = new SuccessorInfo(transition.NextState.ProgramCounter, transition.EdgeType);
                    result.RegisterSuccessor(instruction, successor);
                    
                    // Schedule transition for further processing.
                    agenda.Push(transition.NextState);
                }
            }
            finally
            {
                arrayPool.Return(transitionsBuffer);
            }
        }

        private void DetermineBlockHeaders(in GraphBuilderContext context)
        {
            var result = context.Result;
            var arrayPool = ArrayPool<SuccessorInfo>.Shared;
            var successorBuffer = arrayPool.Rent(2);

            try
            {
                foreach (var instruction in result.GetAllInstructions())
                {
                    // Block headers are introduced by branches only.
                    if ((Architecture.GetFlowControl(instruction) & InstructionFlowControl.CanBranch) == 0)
                        continue;
                    
                    long offset = Architecture.GetOffset(instruction);
                    long size = Architecture.GetSize(instruction);

                    // Make sure the successor buffer is big enough.
                    int successorCount = result.GetSuccessorCount(offset);
                    if (successorBuffer.Length < successorCount)
                    {
                        arrayPool.Return(successorBuffer);
                        successorBuffer = arrayPool.Rent(successorCount);
                    }

                    // Get successors.
                    var successorBufferSlice = new Span<SuccessorInfo>(successorBuffer, 0, successorCount);
                    int actualSuccessorCount = result.GetSuccessors(offset, successorBufferSlice);
                    if (actualSuccessorCount > successorCount)
                        throw new InvalidOperationException();
                        
                    // Register all branch targets as block headers.
                    for (int i = 0; i < actualSuccessorCount; i++)
                        result.BlockHeaders.Add(successorBufferSlice[i].DestinationAddress);

                    // Mark end of current block.
                    result.BlockHeaders.Add(offset + size);
                }
            }
            finally
            {
                arrayPool.Return(successorBuffer);
            }
        }

        private readonly ref struct GraphBuilderContext
        {
            public GraphBuilderContext(IInstructionSetArchitecture<TInstruction> architecture)
            {
                Result = new InstructionTraversalResult<TInstruction>(architecture);
                RecordedStates = new Dictionary<long, SymbolicProgramState<TInstruction>>();
            }

            public IDictionary<long, SymbolicProgramState<TInstruction>> RecordedStates
            {
                get;
            }

            public InstructionTraversalResult<TInstruction> Result
            {
                get;
            }
        }

    }
}