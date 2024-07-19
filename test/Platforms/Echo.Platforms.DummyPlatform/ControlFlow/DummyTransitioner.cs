using System;
using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.DataFlow.Construction;
using Echo.DataFlow.Emulation;
using Echo.Platforms.DummyPlatform.Code;

namespace Echo.Platforms.DummyPlatform.ControlFlow
{
    public class DummyTransitioner : StateTransitioner<DummyInstruction>
    {
        public DummyTransitioner()
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

        public override void GetTransitions(
            in SymbolicProgramState<DummyInstruction> currentState, 
            in DummyInstruction instruction, 
            IList<StateTransition<DummyInstruction>> transitionsBuffer)
        {
            var nextState = ApplyDefaultBehaviour(currentState, instruction);

            switch (instruction.OpCode)
            {
                case DummyOpCode.Op:
                case DummyOpCode.Push:
                case DummyOpCode.Pop:
                case DummyOpCode.Get:
                case DummyOpCode.Set:
                    GetFallthroughTransitions(nextState, transitionsBuffer);
                    break;
                
                case DummyOpCode.Jmp:
                    GetJumpTransitions(nextState, instruction, transitionsBuffer);
                    break;
                
                case DummyOpCode.JmpCond:
                    GetJumpCondTransitions(nextState, instruction, transitionsBuffer);
                    break;
                
                case DummyOpCode.PushOffset:
                    GetPushOffsetTransitions(nextState, instruction, transitionsBuffer);
                    break;
                
                case DummyOpCode.Ret:
                    break;
                
                case DummyOpCode.Switch:
                    GetSwitchTransitions(nextState, instruction, transitionsBuffer);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void GetFallthroughTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            IList<StateTransition<DummyInstruction>> transitionsBuffer)
        {
            transitionsBuffer.Add(new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough));
        }

        private static void GetJumpTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction,
            IList<StateTransition<DummyInstruction>> transitionsBuffer)
        {
            transitionsBuffer.Add(new StateTransition<DummyInstruction>(
                nextState.WithProgramCounter((long) instruction.Operands[0]),
                ControlFlowEdgeType.Unconditional
            ));
        }

        private static void GetJumpCondTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction, 
            IList<StateTransition<DummyInstruction>> transitionsBuffer)
        {
            var branchState = nextState.WithProgramCounter((long) instruction.Operands[0]);
            transitionsBuffer.Add(new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.Conditional));
            transitionsBuffer.Add(new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough));
        }

        private static void GetSwitchTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction,
            IList<StateTransition<DummyInstruction>> transitionsBuffer)
        {
            var targets = (IList<long>) instruction.Operands[0];
            for (int i = 0; i < targets.Count; i++)
            {
                var branchState = nextState.WithProgramCounter(targets[i]);
                transitionsBuffer.Add(new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.Conditional));
            }

            transitionsBuffer.Add(new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough));
        }

        private static void GetPushOffsetTransitions(
            in SymbolicProgramState<DummyInstruction> nextState,
            DummyInstruction instruction, 
            IList<StateTransition<DummyInstruction>> successorBuffer)
        {
            var branchState = nextState.WithProgramCounter((long) instruction.Operands[0]);
            successorBuffer.Add(new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough));
            successorBuffer.Add(new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.None));
        }
    }
}