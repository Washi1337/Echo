using System;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Code;
using Echo.DataFlow.Emulation;
using Iced.Intel;

namespace Echo.Platforms.Iced
{
    /// <summary>
    /// Provides an implementation of the <see cref="IStateTransitionResolver{TInstruction}"/> interface, that
    /// implements the state transitioning for the x86 instruction set.
    /// </summary>
    public class X86StateTransitionResolver : StateTransitionResolverBase<Instruction>
    {
        /// <summary>
        /// Creates a new instance of <see cref="X86StateTransitionResolver"/>.
        /// </summary>
        /// <param name="architecture">The x86 architecture instance.</param>
        public X86StateTransitionResolver(IInstructionSetArchitecture<Instruction> architecture)
            : base(architecture)
        {
        }

        /// <inheritdoc />
        public override int GetTransitionCount(SymbolicProgramState<Instruction> currentState,
            in Instruction instruction)
        {   
            switch (instruction.FlowControl)
            {
                case FlowControl.ConditionalBranch:
                    return 2;
                
                case FlowControl.IndirectBranch: 
                //TODO: Try inferring indirect branch from data flow graph.
                    
                case FlowControl.Return:
                    return 0;
                
                default:
                    return 1;
            }
        }

        /// <inheritdoc />
        public override int GetTransitions(SymbolicProgramState<Instruction> currentState,
            in Instruction instruction,
            Span<StateTransition<Instruction>> transitionBuffer)
        {
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);
            
            switch (instruction.FlowControl)
            {
                case FlowControl.UnconditionalBranch:
                    return GetUnconditionalBranchTransitions(instruction, nextState, transitionBuffer);

                case FlowControl.ConditionalBranch:
                    return GetConditionalBranchTransitions(instruction, nextState, transitionBuffer);
                
                case FlowControl.IndirectBranch: 
                    //TODO: Try inferring indirect branch from data flow graph.
                    
                case FlowControl.Return:
                    return 0;
                
                default:
                    return GetFallthroughTransitions(nextState, transitionBuffer);
            }
        }
        
        private static int GetUnconditionalBranchTransitions(
            Instruction instruction,
            SymbolicProgramState<Instruction> nextState, 
            Span<StateTransition<Instruction>> successorBuffer)
        {
            nextState.ProgramCounter = (long) instruction.NearBranchTarget;
            successorBuffer[0] = new StateTransition<Instruction>(nextState, ControlFlowEdgeType.Unconditional);
            return 1;
        }

        private static int GetConditionalBranchTransitions(
            Instruction instruction,
            SymbolicProgramState<Instruction> nextState,
            Span<StateTransition<Instruction>> successorBuffer)
        {
            var branchState = nextState.Copy();
            branchState.ProgramCounter = (long) instruction.NearBranchTarget;

            successorBuffer[0] = new StateTransition<Instruction>(branchState, ControlFlowEdgeType.Conditional);
            successorBuffer[1] = new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 2;
        }

        private static int GetFallthroughTransitions(
            SymbolicProgramState<Instruction> nextState, 
            Span<StateTransition<Instruction>> successorBuffer)
        {
            successorBuffer[0] = new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 1;
        }

    }
}