using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.DataFlow.Emulation;

namespace Echo.Platforms.AsmResolver
{
    public class CilStateTransitionResolver : StateTransitionResolverBase<CilInstruction>
    {
        public CilStateTransitionResolver()
            : base(CilArchitecture.Instance)
        {
        }

        public override IEnumerable<StateTransition<CilInstruction>> GetTransitions(SymbolicProgramState<CilInstruction> currentState, CilInstruction instruction)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Branch:
                    return GetUnconditionalBranchTransitions(currentState, instruction);

                case CilFlowControl.ConditionalBranch:
                    return GetConditionalBranchTransitions(currentState, instruction);
                
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                case CilFlowControl.Break:
                    return GetFallthroughTransitions(currentState, instruction);
                
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    return Enumerable.Empty<StateTransition<CilInstruction>>();

                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerable<StateTransition<CilInstruction>> GetFallthroughTransitions(
            SymbolicProgramState<CilInstruction> currentState, CilInstruction instruction)
        {
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);

            var transition = new StateTransition<CilInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return new[]
            {
                transition
            };
        }

        private IEnumerable<StateTransition<CilInstruction>> GetUnconditionalBranchTransitions(
            SymbolicProgramState<CilInstruction> currentState, CilInstruction instruction)
        {           
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);
            
            nextState.ProgramCounter = ((ICilLabel) instruction.Operand).Offset;

            var transition = new StateTransition<CilInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return new[]
            {
                transition
            };
        }

        private IEnumerable<StateTransition<CilInstruction>> GetConditionalBranchTransitions(
            SymbolicProgramState<CilInstruction> currentState, CilInstruction instruction)
        {
            var baseNextState = currentState.Copy();
            ApplyDefaultBehaviour(baseNextState, instruction);

            var result = new List<StateTransition<CilInstruction>>
            {
                new StateTransition<CilInstruction>(baseNextState, ControlFlowEdgeType.FallThrough)
            };

            switch (instruction.Operand)
            {
                case ICilLabel singleTarget:
                    var branchState = baseNextState.Copy();
                    branchState.ProgramCounter = singleTarget.Offset;
                    result.Add(new StateTransition<CilInstruction>(branchState, ControlFlowEdgeType.Conditional));
                    break;
                            
                case IEnumerable<ICilLabel> multipleTargets:
                    foreach (var target in multipleTargets)
                    {
                        var nextBranchState = baseNextState.Copy();
                        nextBranchState.ProgramCounter = target.Offset;
                        result.Add(new StateTransition<CilInstruction>(nextBranchState, ControlFlowEdgeType.Conditional));
                    }

                    break;
            }

            return result;
        }
    }
}