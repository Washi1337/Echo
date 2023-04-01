using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Echo.Code;
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
        /// <param name="transitioner">The transition resolver to use for inferring branch targets.</param>
        public SymbolicFlowGraphBuilder(
            IArchitecture<TInstruction> architecture,
            IEnumerable<TInstruction> instructions, 
            IStateTransitioner<TInstruction> transitioner)
        {
            Instructions = new StaticToSymbolicAdapter<TInstruction>(architecture, instructions);
            StateTransitioner = transitioner ?? throw new ArgumentNullException(nameof(transitioner));
        }
        
        /// <summary>
        /// Creates a new symbolic control flow graph builder using the provided program state transition resolver.  
        /// </summary>
        /// <param name="instructions">The instructions to traverse.</param>
        /// <param name="transitioner">The transition resolver to use for inferring branch targets.</param>
        public SymbolicFlowGraphBuilder(
            IStaticInstructionProvider<TInstruction> instructions, 
            IStateTransitioner<TInstruction> transitioner)
        {
            Instructions = new StaticToSymbolicAdapter<TInstruction>(
                instructions ?? throw new ArgumentNullException(nameof(instructions)));
            StateTransitioner = transitioner ?? throw new ArgumentNullException(nameof(transitioner));
        }
        
        /// <summary>
        /// Creates a new symbolic control flow graph builder using the provided program state transition resolver.  
        /// </summary>
        /// <param name="instructions">The instructions to traverse.</param>
        /// <param name="transitioner">The transition resolver to use for inferring branch targets.</param>
        public SymbolicFlowGraphBuilder(
            ISymbolicInstructionProvider<TInstruction> instructions, 
            IStateTransitioner<TInstruction> transitioner)
        {
            Instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
            StateTransitioner = transitioner ?? throw new ArgumentNullException(nameof(transitioner));
        }

        /// <summary>
        /// Gets the instructions to traverse.
        /// </summary>
        public ISymbolicInstructionProvider<TInstruction> Instructions
        {
            get;
        }

        /// <inheritdoc />
        public override IArchitecture<TInstruction> Architecture => Instructions.Architecture;

        /// <summary>
        /// Gets the object responsible for resolving every transition in the program state that an instruction might introduce. 
        /// </summary>
        public IStateTransitioner<TInstruction> StateTransitioner
        {
            get;
        }

        /// <inheritdoc />
        protected override IInstructionTraversalResult<TInstruction> CollectInstructions(
            long entrypoint, IEnumerable<long> knownBlockHeaders)
        {
            using var context = new GraphBuilderContext(Architecture);
            long[] blockHeaders = knownBlockHeaders as long[] ?? knownBlockHeaders.ToArray();
            
            // Perform traversal.
            TraverseInstructions(context, entrypoint, blockHeaders);
            
            // Register known block headers.
            context.Result.BlockHeaders.Add(entrypoint);
            context.Result.BlockHeaders.UnionWith(blockHeaders);
            
            // Infer remaining block headers.
            DetermineBlockHeaders(context);
            
            return context.Result;
        }

        private void TraverseInstructions(GraphBuilderContext context, long entrypoint, IEnumerable<long> knownBlockHeaders)
        {
            var agenda = new Stack<SymbolicProgramState<TInstruction>>();
            foreach (var header in knownBlockHeaders)
                agenda.Push(StateTransitioner.GetInitialState(header));
            agenda.Push(StateTransitioner.GetInitialState(entrypoint));

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
            bool changed;
            
            if (context.RecordedStates.TryGetValue(currentState.ProgramCounter, out var recordedState))
            {
                // We are revisiting this address, merge program states.
                changed = recordedState.MergeStates(currentState, out currentState);
                if (changed)
                    context.RecordedStates[currentState.ProgramCounter] = currentState;
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
            int transitionCount = StateTransitioner.GetTransitionCount(currentState, instruction);
            var transitionsBuffer = context.GetTransitionsBuffer(transitionCount);

            // Read transitions.
            var transitionsBufferSlice = transitionsBuffer.AsSpan(0, transitionCount);
            int actualTransitionCount = StateTransitioner.GetTransitions(currentState, instruction, transitionsBufferSlice);
            if (actualTransitionCount > transitionCount)
            {
                // Sanity check: This should only happen if the transition resolver contains a bug.
                throw new ArgumentException(
                    "The number of transitions that was returned by the transition resolver is inconsistent "
                    + "with the number of actual written transitions.");
            }

            for (int i = 0; i < actualTransitionCount; i++)
            {
                var transition = transitionsBufferSlice[i];

                if (transition.IsRealEdge)
                {
                    // Translate transition into successor info, register it and schedule it for processing.
                    var successor = new SuccessorInfo(transition.NextState.ProgramCounter, transition.EdgeType);
                    result.RegisterSuccessor(instruction, successor);
                }
                else
                {
                    // Transition describes the discovery of a new block header, but does not transfer control to
                    // it directly. Only register it as a new block header.
                    context.Result.BlockHeaders.Add(transition.NextState.ProgramCounter);
                }

                // Schedule transition for further processing.
                agenda.Push(transition.NextState);
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
                    var successorBufferSlice = successorBuffer.AsSpan(0, successorCount);
                    int actualSuccessorCount = result.GetSuccessors(offset, successorBufferSlice);
                    if (actualSuccessorCount > successorCount)
                    {
                        // Sanity check: this should only happen if the InstructionTraversalResult has a bug.
                        throw new ArgumentException(
                            "The number of successors that was returned by the instruction traversal result is "
                            + "inconsistent with the number of actual written successors.");
                    }
                        
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

        private sealed class GraphBuilderContext : IDisposable
        {
            private readonly ArrayPool<StateTransition<TInstruction>> _transitionsBufferPool;
            private StateTransition<TInstruction>[] _transitionsBuffer;

            internal GraphBuilderContext(IArchitecture<TInstruction> architecture)
            {
                Result = new InstructionTraversalResult<TInstruction>(architecture);
                RecordedStates = new Dictionary<long, SymbolicProgramState<TInstruction>>();

                _transitionsBufferPool = ArrayPool<StateTransition<TInstruction>>.Shared;

                // Most common case is at most 2 transitions per instruction.
                _transitionsBuffer = _transitionsBufferPool.Rent(2);
            }

            internal InstructionTraversalResult<TInstruction> Result
            {
                get;
            }

            internal IDictionary<long, SymbolicProgramState<TInstruction>> RecordedStates
            {
                get;
            }

            internal StateTransition<TInstruction>[] GetTransitionsBuffer(int minimalSize)
            {
                if (_transitionsBuffer.Length < minimalSize)
                {
                    _transitionsBufferPool.Return(_transitionsBuffer);
                    _transitionsBuffer = _transitionsBufferPool.Rent(minimalSize);
                }

                return _transitionsBuffer;
            }

            public void Dispose()
            {
                if (_transitionsBuffer is null)
                    return;

                _transitionsBufferPool.Return(_transitionsBuffer);
                _transitionsBuffer = null;
            }
        }
    }
}