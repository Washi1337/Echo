using System;
using System.Collections.Generic;
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
        public override IEnumerable<StateTransition<Instruction>> GetTransitions(
            SymbolicProgramState<Instruction> currentState, 
            Instruction instruction)
        {
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);
            
            switch (instruction.FlowControl)
            {
                case FlowControl.UnconditionalBranch:
                    return GetUnconditionalBranchTransitions(instruction, nextState);
                
                    break;
                case FlowControl.ConditionalBranch:
                    return GetConditionalBranchTransitions(instruction, nextState);
                
                case FlowControl.IndirectBranch: 
                    //TODO: Try inferring indirect branch from data flow graph.
                    
                case FlowControl.Return:
                    return Array.Empty<StateTransition<Instruction>>();
                
                default:
                    return GetFallthroughTransitions(nextState);
            }
        }
        
        private static ICollection<StateTransition<Instruction>> GetUnconditionalBranchTransitions(
            Instruction instruction, 
            SymbolicProgramState<Instruction> nextState)
        {
            nextState.ProgramCounter = (long) instruction.NearBranchTarget;
            return new[]
            {
                new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough),
            };
        }

        private static ICollection<StateTransition<Instruction>> GetConditionalBranchTransitions(
            Instruction instruction, 
            SymbolicProgramState<Instruction> nextState)
        {
            var branchState = nextState.Copy();
            branchState.ProgramCounter = (long) instruction.NearBranchTarget;

            return new[]
            {
                new StateTransition<Instruction>(branchState, ControlFlowEdgeType.Conditional),
                new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough)
            };
        }

        private static ICollection<StateTransition<Instruction>> GetFallthroughTransitions(
            SymbolicProgramState<Instruction> nextState)
        {
            return new[]
            {
                new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough),
            };
        }

    }
}