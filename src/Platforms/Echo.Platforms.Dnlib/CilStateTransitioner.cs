using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.DataFlow;
using Echo.DataFlow.Construction;
using Echo.DataFlow.Emulation;
using DnlibCode = dnlib.DotNet.Emit.Code;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Provides an implementation of a state transition resolver for the CIL instruction set.
    /// </summary>
    public class CilStateTransitioner : StateTransitioner<Instruction>
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
                
                if (handler.HandlerStart is not null && handler.HandlerStart.Offset == entrypointAddress
                    || handler.FilterStart is not null && handler.FilterStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSourceNode<Instruction>(handler);
                }

                if (exceptionSource is { })
                {
                    DataFlowGraph.Nodes.Add(exceptionSource);
                    result = result.Push(new SymbolicValue<Instruction>(
                        new StackDataSource<Instruction>(exceptionSource)));
                    break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override void GetTransitions(
            in SymbolicProgramState<Instruction> currentState, 
            in Instruction instruction, 
            IList<StateTransition<Instruction>> transitionsBuffer)
        {
            // Multiplex based on flow control.
            
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Call when instruction.OpCode.Code == DnlibCode.Jmp:
                case FlowControl.Return:
                case FlowControl.Throw:
                    Terminate(currentState, instruction);
                    break;
                
                case FlowControl.Branch:
                    UnconditionalBranch(currentState, instruction, transitionsBuffer);
                    break;

                case FlowControl.Cond_Branch:
                    ConditionalBranch(currentState, instruction, transitionsBuffer);
                    break;
                
                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                case FlowControl.Break:
                    FallThrough(currentState, instruction, transitionsBuffer);
                    break;
                
                case FlowControl.Phi:
                    throw new NotSupportedException();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Terminate(in SymbolicProgramState<Instruction> currentState, in Instruction instruction)
        {
            // Note: we still perform the transition, to record the final dependencies that a throw or a ret might have. 
            ApplyDefaultBehaviour(currentState, instruction);
        }

        private void FallThrough(
            in SymbolicProgramState<Instruction> currentState,
            Instruction instruction,
            IList<StateTransition<Instruction>> transitionsBuffer)
        {
            // Fallthrough instructions just transform the state normally.
            var nextState = ApplyDefaultBehaviour(currentState, instruction);
            transitionsBuffer.Add(new StateTransition<Instruction>(nextState, ControlFlowEdgeType.FallThrough));
        }

        private void UnconditionalBranch(
            in SymbolicProgramState<Instruction> currentState, 
            Instruction instruction,
            IList<StateTransition<Instruction>> transitionsBuffer)
        {
            // Unconditional branches are similar to normal fallthrough, except they change the program counter.
            var nextState = ApplyDefaultBehaviour(currentState, instruction)
                .WithProgramCounter(((Instruction) instruction.Operand!).Offset);

            transitionsBuffer.Add(new StateTransition<Instruction>(nextState, ControlFlowEdgeType.Unconditional));
        }

        private void ConditionalBranch(
            in SymbolicProgramState<Instruction> currentState,
            Instruction instruction,
            IList<StateTransition<Instruction>> transitionsBuffer)
        {
            // Conditional branches result in multiple possible transitions that could happen.
            var baseNextState = ApplyDefaultBehaviour(currentState, instruction);

            // Define the transition if the branch was not taken. (this is a normal fall through transition).
            transitionsBuffer.Add(new StateTransition<Instruction>(baseNextState, ControlFlowEdgeType.FallThrough));

            // CIL conditional branches can have a single target or multiple targets. Fork the next state, and change
            // the program counters to their branch targets.
            
            switch (instruction.Operand)
            {
                case Instruction singleTarget:
                    var branchState = baseNextState.WithProgramCounter(singleTarget.Offset);
                    transitionsBuffer.Add(new StateTransition<Instruction>(branchState, ControlFlowEdgeType.Conditional));
                    break;
                            
                case IList<Instruction> multipleTargets:
                    for (int i = 0; i < multipleTargets.Count; i++)
                    {
                        var nextBranchState = baseNextState.WithProgramCounter(multipleTargets[i].Offset);
                        transitionsBuffer.Add(new StateTransition<Instruction>(
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