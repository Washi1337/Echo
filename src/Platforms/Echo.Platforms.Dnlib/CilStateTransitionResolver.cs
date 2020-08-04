using System;
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
                var exceptionSource = default(ExternalDataSourceNode<Instruction>);
                if (eh.HandlerStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSourceNode<Instruction>(
                        -(long) eh.HandlerStart.Offset,
                        $"HandlerException_{eh.HandlerStart.Offset:X4}");
                }
                else if (eh.FilterStart != null && eh.FilterStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSourceNode<Instruction>(
                        -(long) eh.FilterStart.Offset,
                        $"FilterException_{eh.FilterStart.Offset:X4}");
                }

                if (exceptionSource is {})
                {
                    DataFlowGraph.Nodes.Add(exceptionSource);
                    result.Stack.Push(new SymbolicValue<Instruction>(new DataSource<Instruction>(exceptionSource)));
                    break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override int GetTransitionCount(SymbolicProgramState<Instruction> currentState,
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
                
                case FlowControl.Cond_Branch when instruction.OpCode.Code == Code.Switch:
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
        public override int GetTransitions(SymbolicProgramState<Instruction> currentState,
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
                    transitionBuffer[0] = Next(currentState, instruction);
                    return 1;

                case FlowControl.Branch:
                    transitionBuffer[0] = Branch(false, currentState, instruction);
                    return 1;

                case FlowControl.Cond_Branch when instruction.OpCode.Code == Code.Switch:
                    var targets = (Instruction[]) instruction.Operand;
                    for (int i = 0; i < targets.Length; i++)
                    {
                        var nextState = currentState.Copy();
                        ApplyDefaultBehaviour(nextState, instruction);
                        nextState.ProgramCounter = targets[i].Offset;
                        transitionBuffer[i] = new StateTransition<Instruction>(nextState, ControlFlowEdgeType.Conditional);
                    }

                    transitionBuffer[targets.Length] = Next(currentState, instruction);
                    return targets.Length + 1;

                case FlowControl.Cond_Branch:
                    transitionBuffer[0] = Branch(true, currentState, instruction);
                    transitionBuffer[1] = Next(currentState, instruction);
                    return 2;

                case FlowControl.Return:
                case FlowControl.Throw:
                    ApplyDefaultBehaviour(currentState.Copy(), instruction);
                    return 0;

                case FlowControl.Phi:
                    throw new NotSupportedException("There are no known instructions with Phi control flow");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private StateTransition<Instruction> Next(SymbolicProgramState<Instruction> currentState, Instruction instruction)
        {
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);
            return new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough);
        }

        private StateTransition<Instruction> Branch(
            bool conditional, 
            SymbolicProgramState<Instruction> currentState, 
            Instruction instruction)
        {
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);
            nextState.ProgramCounter = ((Instruction) instruction.Operand).Offset;
            return new StateTransition<Instruction>(nextState, conditional
                ? ControlFlowEdgeType.Conditional
                : ControlFlowEdgeType.FallThrough);
        }
    }
}