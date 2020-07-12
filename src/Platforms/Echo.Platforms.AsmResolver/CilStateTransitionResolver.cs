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
        public override IEnumerable<StateTransition<CilInstruction>> GetTransitions(SymbolicProgramState<CilInstruction> currentState, CilInstruction instruction)
        {
            // Multiplex based on flow control.
            
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Branch:
                    return GetUnconditionalBranchTransitions(currentState, instruction);

                case CilFlowControl.ConditionalBranch:
                    return GetConditionalBranchTransitions(currentState, instruction);
                
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                case CilFlowControl.Break:
                    return GetFallthroughTransitions(currentState, instruction);
                
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    return Enumerable.Empty<StateTransition<CilInstruction>>();

                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerable<StateTransition<CilInstruction>> GetFallthroughTransitions(
            SymbolicProgramState<CilInstruction> currentState, CilInstruction instruction)
        {
            // Fallthrough instructions just transform the state normally.
            
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);

            var transition = new StateTransition<CilInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return new[]
            {
                transition
            };
        }

        private IEnumerable<StateTransition<CilInstruction>> GetUnconditionalBranchTransitions(
            SymbolicProgramState<CilInstruction> currentState, CilInstruction instruction)
        {           
            // Unconditional branches are similar to normal fallthrough, except they change the program counter.
            
            var nextState = currentState.Copy();
            ApplyDefaultBehaviour(nextState, instruction);
            
            nextState.ProgramCounter = ((ICilLabel) instruction.Operand).Offset;

            var transition = new StateTransition<CilInstruction>(nextState, ControlFlowEdgeType.FallThrough);
            return new[]
            {
                transition
            };
        }

        private IEnumerable<StateTransition<CilInstruction>> GetConditionalBranchTransitions(
            SymbolicProgramState<CilInstruction> currentState, CilInstruction instruction)
        {
            // Conditional branches result in multiple possible transitions that could happen.
            
            var baseNextState = currentState.Copy();
            ApplyDefaultBehaviour(baseNextState, instruction);

            var result = new List<StateTransition<CilInstruction>>
            {
                // Define the transition if the branch was not taken. (this is a normal fall through transition).
                new StateTransition<CilInstruction>(baseNextState, ControlFlowEdgeType.FallThrough)
            };

            // CIL conditional branches can have a single target or multiple targets. Fork the next state, and change
            // the program counters to their branch targets.
            
            switch (instruction.Operand)
            {
                case ICilLabel singleTarget:
                    var branchState = baseNextState.Copy();
                    branchState.ProgramCounter = singleTarget.Offset;
                    result.Add(new StateTransition<CilInstruction>(branchState, ControlFlowEdgeType.Conditional));
                    break;
                            
                case IEnumerable<ICilLabel> multipleTargets:
                    foreach (var target in multipleTargets)
                    {
                        var nextBranchState = baseNextState.Copy();
                        nextBranchState.ProgramCounter = target.Offset;
                        result.Add(new StateTransition<CilInstruction>(nextBranchState, ControlFlowEdgeType.Conditional));
                    }

                    break;
            }

            return result;
        }
    }
}