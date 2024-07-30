using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.Code;
using Echo.DataFlow.Construction;
using Echo.DataFlow.Emulation;
using Iced.Intel;

namespace Echo.Platforms.Iced
{
    /// <summary>
    /// Provides an implementation of the <see cref="IStateTransitioner{TInstruction}"/> interface, that
    /// implements the state transitioning for the x86 instruction set.
    /// </summary>
    public class X86StateTransitioner : StateTransitioner<Instruction>
    {
        /// <summary>
        /// Creates a new instance of <see cref="X86StateTransitioner"/>.
        /// </summary>
        /// <param name="architecture">The x86 architecture instance.</param>
        public X86StateTransitioner(IArchitecture<Instruction> architecture)
            : base(architecture)
        {
        }

        /// <inheritdoc />
        public override void GetTransitions(
            in SymbolicProgramState<Instruction> currentState, 
            in Instruction instruction, 
            IList<StateTransition<Instruction>> transitionsBuffer)
        {
            var nextState = ApplyDefaultBehaviour(currentState, instruction);
            
            switch (instruction.FlowControl)
            {
                case FlowControl.UnconditionalBranch:
                    UnconditionalBranch(instruction, nextState, transitionsBuffer);
                    break;
                
                case FlowControl.ConditionalBranch:
                    ConditionalBranch(instruction, nextState, transitionsBuffer);
                    break;
                
                case FlowControl.IndirectBranch: 
                    //TODO: Try inferring indirect branch from data flow graph.
                    
                case FlowControl.Return:
                    break;
                
                default:
                    FallThrough(nextState, transitionsBuffer);
                    break;
            }
        }

        private static void UnconditionalBranch(
            in Instruction instruction,
            in SymbolicProgramState<Instruction> nextState, 
            IList<StateTransition<Instruction>> successorBuffer)
        {
            var branchState = nextState.WithProgramCounter((long) instruction.NearBranchTarget);
            successorBuffer.Add(new StateTransition<Instruction>(branchState, ControlFlowEdgeType.Unconditional));
        }

        private static void ConditionalBranch(
            in Instruction instruction,
            in SymbolicProgramState<Instruction> nextState,
            IList<StateTransition<Instruction>> successorBuffer)
        {
            var branchState = nextState.WithProgramCounter((long) instruction.NearBranchTarget);
            successorBuffer.Add(new StateTransition<Instruction>(branchState, ControlFlowEdgeType.Conditional));
            successorBuffer.Add(new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough));
        }

        private static void FallThrough(
            in SymbolicProgramState<Instruction> nextState, 
            IList<StateTransition<Instruction>> successorBuffer)
        {
            successorBuffer.Add(new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough));
        }
    }
}