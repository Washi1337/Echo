using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.DataFlow;
using Echo.DataFlow.Emulation;
using Echo.DataFlow.Values;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides an implementation of a state transition resolver for the CIL instruction set.
    /// </summary>
    public class CilStateTransitionResolver : StateTransitionResolverBase<CilInstruction>
    {
        private readonly CilArchitecture _architecture;

        /// <summary>
        /// Creates a new instance of the <see cref="CilStateTransitionResolver"/> class.
        /// </summary>
        /// <param name="architecture">The CIL architecture variant to compute state transitions for.</param>
        public CilStateTransitionResolver(CilArchitecture architecture)
            : base(architecture)
        {
            _architecture = architecture;
        }

        /// <inheritdoc />
        public override SymbolicProgramState<CilInstruction> GetInitialState(long entrypointAddress)
        {
            var result = base.GetInitialState(entrypointAddress);
            
            foreach (var eh in _architecture.MethodBody.ExceptionHandlers)
            {
                var exceptionSource = default(ExternalDataSource<CilInstruction>);
                if (eh.HandlerStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSource<CilInstruction>(
                        -(long) eh.HandlerStart.Offset,
                        $"HandlerException_{eh.HandlerStart.Offset:X4}");
                }
                else if (eh.FilterStart != null && eh.FilterStart.Offset == entrypointAddress)
                {
                    exceptionSource = new ExternalDataSource<CilInstruction>(
                        -(long) eh.FilterStart.Offset,
                        $"FilterException_{eh.FilterStart.Offset:X4}");
                }

                if (exceptionSource is {})
                {
                    DataFlowGraph.Nodes.Add(exceptionSource);
                    result.Stack.Push(new SymbolicValue<CilInstruction>(exceptionSource));
                    break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override int GetTransitionCount(SymbolicProgramState<CilInstruction> currentState,
            in CilInstruction instruction)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                case CilFlowControl.Break:
                case CilFlowControl.Branch:
                    return 1;

                case CilFlowControl.ConditionalBranch when instruction.OpCode.Code == CilCode.Switch:
                    return ((ICollection<ICilLabel>) instruction.Operand).Count + 1;
                
                case CilFlowControl.ConditionalBranch:
                    return 2;
                
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    return 0;

                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override int GetTransitions(SymbolicProgramState<CilInstruction> currentState,
            in CilInstruction instruction,
            Span<StateTransition<CilInstruction>> transitionBuffer)
        {
            // Multiplex based on flow control.
            
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Branch:
                    return GetUnconditionalBranchTransitions(currentState, instruction, transitionBuffer);

                case CilFlowControl.ConditionalBranch:
                    return GetConditionalBranchTransitions(currentState, instruction, transitionBuffer);
                
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                case CilFlowControl.Break:
                    return GetFallthroughTransitions(currentState, instruction, transitionBuffer);
                
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    return ProcessTerminatingTransition(currentState, instruction);

                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int ProcessTerminatingTransition(SymbolicProgramState<CilInstruction> currentState, in CilInstruction instruction)
        {
            // Note: we still perform the transition, to record the final dependencies that a throw or a ret might have. 
            
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);

            return 0;
        }

        private int GetFallthroughTransitions(
            SymbolicProgramState<CilInstruction> currentState,
            CilInstruction instruction,
            Span<StateTransition<CilInstruction>> successorBuffer)
        {
            // Fallthrough instructions just transform the state normally.
            
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);

            successorBuffer[0] = new StateTransition<CilInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 1;
        }

        private int GetUnconditionalBranchTransitions(
            SymbolicProgramState<CilInstruction> currentState, 
            CilInstruction instruction,
            Span<StateTransition<CilInstruction>> successorBuffer)
        {           
            // Unconditional branches are similar to normal fallthrough, except they change the program counter.
            
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);
            
            nextState.ProgramCounter = ((ICilLabel) instruction.Operand).Offset;

            successorBuffer[0] = new StateTransition<CilInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return 1;
        }

        private int GetConditionalBranchTransitions(
            SymbolicProgramState<CilInstruction> currentState,
            CilInstruction instruction,
            Span<StateTransition<CilInstruction>> successorBuffer)
        {
            // Conditional branches result in multiple possible transitions that could happen.
            
            var baseNextState = currentState.Copy();
            ApplyDefaultBehaviour(baseNextState, instruction);

            // Define the transition if the branch was not taken. (this is a normal fall through transition).
            successorBuffer[0] = new StateTransition<CilInstruction>(baseNextState, ControlFlowEdgeType.FallThrough);

            // CIL conditional branches can have a single target or multiple targets. Fork the next state, and change
            // the program counters to their branch targets.
            
            switch (instruction.Operand)
            {
                case ICilLabel singleTarget:
                    var branchState = baseNextState.Copy();
                    branchState.ProgramCounter = singleTarget.Offset;
                    successorBuffer[1] = new StateTransition<CilInstruction>(branchState, ControlFlowEdgeType.Conditional);
                    return 2;
                            
                case IList<ICilLabel> multipleTargets:
                    for (int i = 0; i < multipleTargets.Count; i++)
                    {
                        var nextBranchState = baseNextState.Copy();
                        nextBranchState.ProgramCounter = multipleTargets[i].Offset;
                        
                        successorBuffer[i + 1] = new StateTransition<CilInstruction>(
                            nextBranchState, 
                            ControlFlowEdgeType.Conditional);
                    }
                    
                    return multipleTargets.Count + 1;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}