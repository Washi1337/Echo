using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.DataFlow;
using Echo.DataFlow.Emulation;
using Echo.DataFlow.Values;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Provides an implementation of a state transition resolver for the CIL instruction set.
    /// </summary>
    public class CilStateTransitionResolver : StateTransitionResolverBase<Instruction>
    {
        private readonly CilArchitecture _architecture;

        /// <summary>
        /// Creates a new instance of the <see cref="CilStateTransitionResolver"/> class.
        /// </summary>
        /// <param name="architecture">The CIL architecture variant to compute state transitions for.</param>
        public CilStateTransitionResolver(CilArchitecture architecture) : base(architecture)
        {
            _architecture = architecture;
        }

        /// <inheritdoc />
        public override SymbolicProgramState<Instruction> GetInitialState(long entrypointAddress)
        {
            var result = base.GetInitialState(entrypointAddress);
            
            foreach (var eh in _architecture.MethodBody.ExceptionHandlers)
            {
                var exceptionSource = default(ExternalDataSource<Instruction>);
                if (eh.HandlerStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSource<Instruction>(
                        -(long) eh.HandlerStart.Offset,
                        $"HandlerException_{eh.HandlerStart.Offset:X4}");
                }
                else if (eh.FilterStart != null && eh.FilterStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSource<Instruction>(
                        -(long) eh.FilterStart.Offset,
                        $"FilterException_{eh.FilterStart.Offset:X4}");
                }

                if (exceptionSource is {})
                {
                    DataFlowGraph.Nodes.Add(exceptionSource);
                    result.Stack.Push(new SymbolicValue<Instruction>(exceptionSource));
                    break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override IEnumerable<StateTransition<Instruction>> GetTransitions(SymbolicProgramState<Instruction> currentState, Instruction instruction)
        {
            // Multiplex based on flow control.

            switch (instruction.OpCode.FlowControl)
            {

                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                case FlowControl.Break:
                    return new[] {Next()};

                case FlowControl.Branch:
                    return new[] {Branch(false)};

                case FlowControl.Cond_Branch when instruction.OpCode.Code == Code.Switch:
                    return Switch().Append(Next());

                case FlowControl.Cond_Branch:
                    return new[] {Branch(true), Next()};

                case FlowControl.Return:
                case FlowControl.Throw:
                    return Array.Empty<StateTransition<Instruction>>();

                case FlowControl.Phi:
                    throw new NotSupportedException("There are no known instructions with Phi control flow");

                default:
                    throw new ArgumentOutOfRangeException();
            }

            StateTransition<Instruction> Next()
            {
                var nextState = currentState.Copy();
                ApplyDefaultBehaviour(nextState, instruction);
                return new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough);
            }

            StateTransition<Instruction> Branch(bool conditional)
            {
                var nextState = currentState.Copy();
                ApplyDefaultBehaviour(nextState, instruction);
                nextState.ProgramCounter = ((Instruction) instruction.Operand).Offset;
                return new StateTransition<Instruction>(nextState,
                    conditional ? ControlFlowEdgeType.Conditional : ControlFlowEdgeType.FallThrough);
            }

            IEnumerable<StateTransition<Instruction>> Switch() => ((Instruction[]) instruction.Operand).Select(i =>
            {
                var nextState = currentState.Copy();
                ApplyDefaultBehaviour(nextState, instruction);
                nextState.ProgramCounter = i.Offset;
                return new StateTransition<Instruction>(nextState, ControlFlowEdgeType.Conditional);
            });
        }
    }
}