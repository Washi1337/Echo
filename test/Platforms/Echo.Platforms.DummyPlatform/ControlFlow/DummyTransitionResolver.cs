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

        public override IEnumerable<StateTransition<DummyInstruction>> GetTransitions(SymbolicProgramState<DummyInstruction> currentState, DummyInstruction instruction)
        {
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);

            return instruction.OpCode switch
            {
                DummyOpCode.Op => GetFallthroughTransitions(nextState),
                DummyOpCode.Push => GetFallthroughTransitions(nextState),
                DummyOpCode.Pop => GetFallthroughTransitions(nextState),
                DummyOpCode.Jmp => GetJumpTransitions(nextState, instruction),
                DummyOpCode.JmpCond => GetJumpCondTransitions(nextState, instruction),
                DummyOpCode.Ret => Enumerable.Empty<StateTransition<DummyInstruction>>(),
                DummyOpCode.Switch => GetSwitchTransitions(nextState, instruction),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static IEnumerable<StateTransition<DummyInstruction>> GetFallthroughTransitions(SymbolicProgramState<DummyInstruction> nextState)
        {
            return new[]
            {
                new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough),
            };
        }

        private IEnumerable<StateTransition<DummyInstruction>> GetJumpTransitions(SymbolicProgramState<DummyInstruction> nextState, DummyInstruction instruction)
        {
            nextState.ProgramCounter = (long) instruction.Operands[0];
            return new[]
            {
                new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough),
            };
        }

        private IEnumerable<StateTransition<DummyInstruction>> GetJumpCondTransitions(SymbolicProgramState<DummyInstruction> nextState, DummyInstruction instruction)
        {
            var branchState = nextState.Copy();
            branchState.ProgramCounter = (long) instruction.Operands[0];
            return new[]
            {
                new StateTransition<DummyInstruction>(branchState, ControlFlowEdgeType.Conditional),
                new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough),
            };
        }

        private IEnumerable<StateTransition<DummyInstruction>> GetSwitchTransitions(SymbolicProgramState<DummyInstruction> nextState, DummyInstruction instruction)
        {
            var result = new List<StateTransition<DummyInstruction>>()
            {
                new StateTransition<DummyInstruction>(nextState, ControlFlowEdgeType.FallThrough),
            };

            foreach (long target in (IEnumerable<long>) instruction.Operands[0])
            {
                var branchState = nextState.Copy();
                branchState.ProgramCounter = target;
            }

            return result;
        }
    }
}