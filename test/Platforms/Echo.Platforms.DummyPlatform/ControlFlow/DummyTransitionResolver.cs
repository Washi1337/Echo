using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Code;
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
        } = SymbolicProgramState<DummyInstruction>.Empty;

        public override SymbolicProgramState<DummyInstruction> GetInitialState(long entrypointAddress)
        {
            return InitialState.WithProgramCounter(entrypointAddress);
        }

        public override int GetTransitionCount(
            in SymbolicProgramState<DummyInstruction> currentState,
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
                case DummyOpCode.PushOffset:
                    return 2;
                
                case DummyOpCode.Ret:
                    return 0;
                
                case DummyOpCode.Switch:
                    return ((ICollection<long>) instruction.Operands[0]).Count + 1;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int GetTransitions(
            in SymbolicProgramState<DummyInstruction> currentState,
            in DummyInstruction instruction,
            Span<StateTransition<DummyInstruction>> transitionBuffer)
        {
            var nextState = ApplyDefaultBehaviour(currentState, instruction);

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
                
                case DummyOpCode.PushOffset:
                    return GetPushOffsetTransitions(nextState, instruction, transitionBuffer);
                
                case DummyOpCode.Ret:
                    return 0;
                
                case DummyOpCode.Switch:
                    return GetSwitchTransitions(nextState, instruction, transitionBuffer);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetFallthroughTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            successorBuffer[0] = new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 1;
        }

        private static int GetJumpTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction,
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            successorBuffer[0] = new StateTransition<DummyInstruction>(
                nextState.WithProgramCounter((long) instruction.Operands[0]),
                ControlFlowEdgeType.Unconditional);
            return 1;
        }

        private static int GetJumpCondTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction, 
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            var branchState = nextState.WithProgramCounter((long) instruction.Operands[0]);
            successorBuffer[0] = new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.Conditional);
            successorBuffer[1] = new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 2;
        }

        private static int GetSwitchTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction,
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            var targets = (IList<long>) instruction.Operands[0];
            for (int i = 0; i < targets.Count; i++)
            {
                var branchState = nextState.WithProgramCounter(targets[i]);
                successorBuffer[i] =  new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.Conditional);
            }

            successorBuffer[targets.Count] = new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return targets.Count + 1;
        }

        private static int GetPushOffsetTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction, 
            Span<StateTransition<DummyInstruction>> successorBuffer)
        {
            var branchState = nextState.WithProgramCounter((long) instruction.Operands[0]);
            successorBuffer[1] = new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            successorBuffer[0] = new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.None);
            return 2;
        }
    }
}