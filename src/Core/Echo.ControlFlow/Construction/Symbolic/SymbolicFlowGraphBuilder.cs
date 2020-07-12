using System;
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
            var blockHeaders = knownBlockHeaders as long[] ?? knownBlockHeaders.ToArray();
            
            var result = TraverseInstructions(entrypoint, blockHeaders);
            result.BlockHeaders.UnionWith(blockHeaders);
            DetermineBlockHeaders(result);
            
            return result;
        }

        private InstructionTraversalResult<TInstruction> TraverseInstructions(
            long entrypoint, IEnumerable<long> knownBlockHeaders)
        {
            var result = new InstructionTraversalResult<TInstruction>();
            
            var recordedStates = new Dictionary<long, SymbolicProgramState<TInstruction>>();
            
            var agenda = new Stack<SymbolicProgramState<TInstruction>>();
            foreach (var header in knownBlockHeaders)
                agenda.Push(TransitionResolver.GetInitialState(header));
            agenda.Push(TransitionResolver.GetInitialState(entrypoint));

            while (agenda.Count > 0)
            {
                // Merge the current state with the recorded states.
                var currentState = agenda.Pop();
                bool recordedStatesChanged = ApplyStateChange(recordedStates, ref currentState);
                
                // If anything changed, we must invalidate the known successors of the current
                // instruction and (re)visit all its successors.
                if (recordedStatesChanged)
                {
                    var instruction = Instructions.GetCurrentInstruction(currentState);
                    var instructionInfo = InvalidateKnownSuccessors(result, currentState, instruction);
                    ResolveAndScheduleSuccessors(currentState, instructionInfo, agenda);
                }
            }

            return result;
        }

        private static bool ApplyStateChange(
            IDictionary<long, SymbolicProgramState<TInstruction>> recordedStates, 
            ref SymbolicProgramState<TInstruction> currentState)
        {
            bool changed = false;
            if (recordedStates.TryGetValue(currentState.ProgramCounter, out var recordedState))
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
                recordedStates.Add(currentState.ProgramCounter, currentState);
                changed = true;
            }

            return changed;
        }

        private static InstructionInfo<TInstruction> InvalidateKnownSuccessors(
            InstructionTraversalResult<TInstruction> result, 
            SymbolicProgramState<TInstruction> currentState,
            TInstruction instruction)
        {
            if (result.Instructions.TryGetValue(currentState.ProgramCounter, out var info))
            {
                info.Successors.Clear();
            }
            else
            {
                info = new InstructionInfo<TInstruction>(instruction, new List<SuccessorInfo>());
                result.Instructions.Add(currentState.ProgramCounter, info);
            }

            return info;
        }

        private void ResolveAndScheduleSuccessors(
            SymbolicProgramState<TInstruction> currentState, 
            InstructionInfo<TInstruction> info,
            Stack<SymbolicProgramState<TInstruction>> agenda)
        {
            foreach (var transition in TransitionResolver.GetTransitions(currentState, info.Instruction))
            {
                info.Successors.Add(new SuccessorInfo(transition.NextState.ProgramCounter, transition.EdgeType));
                agenda.Push(transition.NextState);
            }
        }

        private void DetermineBlockHeaders(InstructionTraversalResult<TInstruction> result)
        {
            foreach (var (instruction, successors) in result.GetAllInstructions())
            {
                if ((Architecture.GetFlowControl(instruction) & InstructionFlowControl.CanBranch) != 0)
                {
                    foreach (var successor in successors)
                        result.BlockHeaders.Add(successor.DestinationAddress);
                    
                    long offset = Architecture.GetOffset(instruction);
                    long size = Architecture.GetSize(instruction);
                    result.BlockHeaders.Add(offset + size);
                }
            }
        }
        
    }
}