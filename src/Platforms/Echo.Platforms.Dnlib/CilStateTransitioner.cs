using System;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.DataFlow;
using Echo.DataFlow.Emulation;
using DnlibCode = dnlib.DotNet.Emit.Code;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Provides an implementation of a state transition resolver for the CIL instruction set.
    /// </summary>
    public class CilStateTransitioner : StateTransitionerBase<Instruction>
    {
        private readonly CilArchitecture _architecture;

        /// <summary>
        /// Creates a new instance of the <see cref="CilStateTransitioner"/> class.
        /// </summary>
        /// <param name="architecture">The CIL architecture variant to compute state transitions for.</param>
        public CilStateTransitioner(CilArchitecture architecture) : base(architecture)
        {
            _architecture = architecture;
        }

        /// <inheritdoc />
        public override SymbolicProgramState<Instruction> GetInitialState(long entrypointAddress)
        {
            var result = base.GetInitialState(entrypointAddress);

            for (int i = 0; i < _architecture.MethodBody.ExceptionHandlers.Count; i++)
            {
                var handler = _architecture.MethodBody.ExceptionHandlers[i];
                if (handler.HandlerType == ExceptionHandlerType.Fault
                    || handler.HandlerType == ExceptionHandlerType.Finally)
                {
                    continue;
                }

                var exceptionSource = default(ExternalDataSourceNode<Instruction>);
                if (handler.HandlerStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSourceNode<Instruction>(
                        -handler.HandlerStart.Offset,
                        $"HandlerException_{handler.HandlerStart.Offset:X4}");
                }
                else if (handler.FilterStart != null && handler.FilterStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSourceNode<Instruction>(
                        -handler.FilterStart.Offset,
                        $"FilterException_{handler.FilterStart.Offset:X4}");
                }

                if (exceptionSource is { })
                {
                    DataFlowGraph.Nodes.Add(exceptionSource);
                    result = result.Push(new SymbolicValue<Instruction>(new StackDataSource<Instruction>(exceptionSource)));
                    break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override int GetTransitionCount(
            in SymbolicProgramState<Instruction> currentState,
            in Instruction instruction)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                case FlowControl.Break:
                case FlowControl.Branch:
                    return 1;
                
                case FlowControl.Cond_Branch when instruction.OpCode.Code == DnlibCode.Switch:
                    var targets = (Instruction[]) instruction.Operand;
                    return targets.Length + 1;
                
                case FlowControl.Cond_Branch:
                    return 2;
                
                case FlowControl.Return:
                case FlowControl.Throw:
                    return 0;

                case FlowControl.Phi:
                    throw new NotSupportedException("There are no known instructions with Phi control flow");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override int GetTransitions(
            in SymbolicProgramState<Instruction> currentState,
            in Instruction instruction,
            Span<StateTransition<Instruction>> transitionBuffer)
        {
            // Multiplex based on flow control.

            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                case FlowControl.Break:
                    transitionBuffer[0] = FallThrough(currentState, instruction);
                    return 1;

                case FlowControl.Branch:
                    transitionBuffer[0] = Branch(false, currentState, instruction);
                    return 1;

                case FlowControl.Cond_Branch when instruction.OpCode.Code == DnlibCode.Switch:
                    var targets = (Instruction[]) instruction.Operand;
                    for (int i = 0; i < targets.Length; i++)
                    {
                        var nextState = ApplyDefaultBehaviour(currentState, instruction)
                            .WithProgramCounter(targets[i].Offset);
                        transitionBuffer[i] = new StateTransition<Instruction>(nextState, ControlFlowEdgeType.Conditional);
                    }

                    transitionBuffer[targets.Length] = FallThrough(currentState, instruction);
                    return targets.Length + 1;

                case FlowControl.Cond_Branch:
                    transitionBuffer[0] = Branch(true, currentState, instruction);
                    transitionBuffer[1] = FallThrough(currentState, instruction);
                    return 2;

                case FlowControl.Return:
                case FlowControl.Throw:
                    ApplyDefaultBehaviour(currentState, instruction);
                    return 0;

                case FlowControl.Phi:
                    throw new NotSupportedException("There are no known instructions with Phi control flow");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private StateTransition<Instruction> FallThrough(
            in SymbolicProgramState<Instruction> currentState, 
            Instruction instruction)
        {
            var nextState = ApplyDefaultBehaviour(currentState, instruction);
            return new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough);
        }

        private StateTransition<Instruction> Branch(
            bool conditional, 
            in SymbolicProgramState<Instruction> currentState, 
            Instruction instruction)
        {
            var nextState = ApplyDefaultBehaviour(currentState, instruction)
                .WithProgramCounter(((Instruction) instruction.Operand).Offset);
            return new StateTransition<Instruction>(nextState, conditional
                ? ControlFlowEdgeType.Conditional
                : ControlFlowEdgeType.Unconditional);
        }
    }
}