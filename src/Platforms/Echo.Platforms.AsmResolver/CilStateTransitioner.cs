using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow;
using Echo.DataFlow;
using Echo.DataFlow.Construction;
using Echo.DataFlow.Emulation;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides an implementation of a state transition resolver for the CIL instruction set.
    /// </summary>
    public class CilStateTransitioner : StateTransitioner<CilInstruction>
    {
        private readonly CilArchitecture _architecture;

        /// <summary>
        /// Creates a new instance of the <see cref="CilStateTransitioner"/> class.
        /// </summary>
        /// <param name="architecture">The CIL architecture variant to compute state transitions for.</param>
        public CilStateTransitioner(CilArchitecture architecture)
            : base(architecture)
        {
            _architecture = architecture;
        }

        /// <inheritdoc />
        public override SymbolicProgramState<CilInstruction> GetInitialState(long entrypointAddress)
        {
            var result = base.GetInitialState(entrypointAddress);

            for (int i = 0; i < _architecture.MethodBody.ExceptionHandlers.Count; i++)
            {
                var handler = _architecture.MethodBody.ExceptionHandlers[i];
                if (handler.HandlerType == CilExceptionHandlerType.Fault
                    || handler.HandlerType == CilExceptionHandlerType.Finally)
                {
                    continue;
                }

                var exceptionSource = default(ExternalDataSourceNode<CilInstruction>);
                
                if (handler.HandlerStart!.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSourceNode<CilInstruction>(
                        -(long) handler.HandlerStart.Offset,
                        $"HandlerException_{handler.HandlerStart.Offset:X4}");
                }
                else if (handler.FilterStart != null && handler.FilterStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSourceNode<CilInstruction>(
                        -(long) handler.FilterStart.Offset,
                        $"FilterException_{handler.FilterStart.Offset:X4}");
                }

                if (exceptionSource is { })
                {
                    DataFlowGraph.Nodes.Add(exceptionSource);
                    result = result.Push(new SymbolicValue<CilInstruction>(
                        new StackDataSource<CilInstruction>(exceptionSource)));
                    break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override void GetTransitions(
            in SymbolicProgramState<CilInstruction> currentState, 
            in CilInstruction instruction, 
            IList<StateTransition<CilInstruction>> transitionsBuffer)
        {
            // Multiplex based on flow control.
            
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Call when instruction.OpCode.Code == CilCode.Jmp:
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    Terminate(currentState, instruction);
                    break;
                
                case CilFlowControl.Branch:
                    UnconditionalBranch(currentState, instruction, transitionsBuffer);
                    break;

                case CilFlowControl.ConditionalBranch:
                    ConditionalBranch(currentState, instruction, transitionsBuffer);
                    break;
                
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                case CilFlowControl.Break:
                    FallThrough(currentState, instruction, transitionsBuffer);
                    break;
                
                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Terminate(in SymbolicProgramState<CilInstruction> currentState, in CilInstruction instruction)
        {
            // Note: we still perform the transition, to record the final dependencies that a throw or a ret might have. 
            ApplyDefaultBehaviour(currentState, instruction);
        }

        private void FallThrough(
            in SymbolicProgramState<CilInstruction> currentState,
            CilInstruction instruction,
            IList<StateTransition<CilInstruction>> transitionsBuffer)
        {
            // Fallthrough instructions just transform the state normally.
            var nextState = ApplyDefaultBehaviour(currentState, instruction);
            transitionsBuffer.Add(new StateTransition<CilInstruction>(nextState, ControlFlowEdgeType.FallThrough));
        }

        private void UnconditionalBranch(
            in SymbolicProgramState<CilInstruction> currentState, 
            CilInstruction instruction,
            IList<StateTransition<CilInstruction>> transitionsBuffer)
        {
            // Unconditional branches are similar to normal fallthrough, except they change the program counter.
            var nextState = ApplyDefaultBehaviour(currentState, instruction)
                .WithProgramCounter(((ICilLabel) instruction.Operand!).Offset);

            transitionsBuffer.Add(new StateTransition<CilInstruction>(nextState, ControlFlowEdgeType.Unconditional));
        }

        private void ConditionalBranch(
            in SymbolicProgramState<CilInstruction> currentState,
            CilInstruction instruction,
            IList<StateTransition<CilInstruction>> transitionsBuffer)
        {
            // Conditional branches result in multiple possible transitions that could happen.
            var baseNextState = ApplyDefaultBehaviour(currentState, instruction);

            // Define the transition if the branch was not taken. (this is a normal fall through transition).
            transitionsBuffer.Add(new StateTransition<CilInstruction>(baseNextState, ControlFlowEdgeType.FallThrough));

            // CIL conditional branches can have a single target or multiple targets. Fork the next state, and change
            // the program counters to their branch targets.
            
            switch (instruction.Operand)
            {
                case ICilLabel singleTarget:
                    var branchState = baseNextState.WithProgramCounter(singleTarget.Offset);
                    transitionsBuffer.Add(new StateTransition<CilInstruction>(branchState, ControlFlowEdgeType.Conditional));
                    break;
                            
                case IList<ICilLabel> multipleTargets:
                    for (int i = 0; i < multipleTargets.Count; i++)
                    {
                        var nextBranchState = baseNextState.WithProgramCounter(multipleTargets[i].Offset);
                        transitionsBuffer.Add(new StateTransition<CilInstruction>(
                            nextBranchState,
                            ControlFlowEdgeType.Conditional
                        ));
                    }

                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}