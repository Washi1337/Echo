using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Code;
using Echo.DataFlow.Emulation;
using Echo.Platforms.DummyPlatform.Code;

namespace Echo.Platforms.DummyPlatform.ControlFlow
{
    public class DummyTransitionResolver : StateTransitionResolverBase<DummyInstruction>
    {
        public DummyTransitionResolver()
            : base(DummyArchitecture.Instance)
        {
        }

        public SymbolicProgramState<DummyInstruction> InitialState
        {
            get;
            set;
        } = new SymbolicProgramState<DummyInstruction>();

        public override SymbolicProgramState<DummyInstruction> GetInitialState(long entrypointAddress)
        {
            var state = InitialState.Copy();
            state.ProgramCounter = entrypointAddress;
            return state;
        }

        public override int GetTransitionCount(SymbolicProgramState<DummyInstruction> currentState,
            in DummyInstruction instruction)
        {
            switch (instruction.OpCode)
            {
                case DummyOpCode.Op:
                case DummyOpCode.Push:
                case DummyOpCode.Pop:
                case DummyOpCode.Get:
                case DummyOpCode.Set:
                case DummyOpCode.Jmp:
                    return 1;
                
                case DummyOpCode.JmpCond:
                    return 2;
                
                case DummyOpCode.Ret:
                    return 0;
                
                case DummyOpCode.Switch:
                    return ((ICollection<long>) instruction.Operands[0]).Count;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int GetTransitions(
            SymbolicProgramState<DummyInstruction> currentState,
            in DummyInstruction instruction, 
            Span<StateTransition<DummyInstruction>> transitionBuffer)
        {
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);

            switch (instruction.OpCode)
            {
                case DummyOpCode.Op:
                case DummyOpCode.Push:
                case DummyOpCode.Pop:
                case DummyOpCode.Get:
                case DummyOpCode.Set:
                    return GetFallthroughTransitions(nextState, transitionBuffer);
                
                case DummyOpCode.Jmp:
                    return GetJumpTransitions(nextState, instruction, transitionBuffer);
                
                case DummyOpCode.JmpCond:
                    return GetJumpCondTransitions(nextState, instruction, transitionBuffer);
                
                case DummyOpCode.Ret:
                    return 0;
                
                case DummyOpCode.Switch:
                    return GetSwitchTransitions(nextState, instruction, transitionBuffer);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetFallthroughTransitions(
            SymbolicProgramState<DummyInstruction> nextState,
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            successorBuffer[0] = new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 1;
        }

        private static int GetJumpTransitions(
            SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction,
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            nextState.ProgramCounter = (long) instruction.Operands[0];
            successorBuffer[0] = new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 1;
        }

        private static int GetJumpCondTransitions(
            SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction, 
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            var branchState = nextState.Copy();
            branchState.ProgramCounter = (long) instruction.Operands[0];
            successorBuffer[0] = new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.Conditional);
            successorBuffer[1] = new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 2;
        }

        private static int GetSwitchTransitions(
            SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction,
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            var targets = (IList<long>) instruction.Operands[0];
            for (int i = 0; i < targets.Count; i++)
            {
                var branchState = nextState.Copy();
                branchState.ProgramCounter = targets[i];
                successorBuffer[i] =  new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.Conditional);
            }

            successorBuffer[targets.Count] = new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return targets.Count;
        }
    }
}