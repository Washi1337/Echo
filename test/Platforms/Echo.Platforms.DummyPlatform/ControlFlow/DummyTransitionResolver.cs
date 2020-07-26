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
            return instruction.OpCode switch
            {
                DummyOpCode.Op => 1,
                DummyOpCode.Push => 1,
                DummyOpCode.Pop => 1,
                DummyOpCode.Jmp => 1,
                DummyOpCode.JmpCond => 2,
                DummyOpCode.Ret => 0,
                DummyOpCode.Switch => ((ICollection<long>) instruction.Operands[0]).Count,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override int GetTransitions(
            SymbolicProgramState<DummyInstruction> currentState,
            in DummyInstruction instruction, 
            Span<StateTransition<DummyInstruction>> transitionBuffer)
        {
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);

            return instruction.OpCode switch
            {
                DummyOpCode.Op => GetFallthroughTransitions(nextState, transitionBuffer),
                DummyOpCode.Push => GetFallthroughTransitions(nextState, transitionBuffer),
                DummyOpCode.Pop => GetFallthroughTransitions(nextState, transitionBuffer),
                DummyOpCode.Jmp => GetJumpTransitions(nextState, instruction, transitionBuffer),
                DummyOpCode.JmpCond => GetJumpCondTransitions(nextState, instruction, transitionBuffer),
                DummyOpCode.Ret => 0,
                DummyOpCode.Switch => GetSwitchTransitions(nextState, instruction, transitionBuffer),
                _ => throw new ArgumentOutOfRangeException()
            };
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