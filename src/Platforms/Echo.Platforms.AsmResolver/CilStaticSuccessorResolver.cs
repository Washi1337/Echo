using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides an implementation of <see cref="IStaticSuccessorResolver{CilInstruction}"/>
    /// </summary>
    public class CilStaticSuccessorResolver : IStaticSuccessorResolver<CilInstruction>
    {
        /// <summary>
        /// Gets a reusable singleton instance of the static successor resolver for the CIL architecture.
        /// </summary>
        public static CilStaticSuccessorResolver Instance
        {
            get;
        } = new CilStaticSuccessorResolver();

        /// <inheritdoc />
        public int GetSuccessorsCount(in CilInstruction instruction)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Break:
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                case CilFlowControl.Branch:
                    return 1;
                
                case CilFlowControl.ConditionalBranch when instruction.OpCode.Code == CilCode.Switch:
                    return ((ICollection<ICilLabel>) instruction.Operand!).Count + 1;
                
                case CilFlowControl.ConditionalBranch:
                    return 2;

                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    return 0;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public int GetSuccessors(in CilInstruction instruction, Span<SuccessorInfo> successorsBuffer)
        {
            // Multiplex based on flow control.
            
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Break:
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                    return GetFallThroughTransitions(instruction, successorsBuffer);
                
                case CilFlowControl.Branch:
                    return GetUnconditionalBranchTransitions(instruction, successorsBuffer);
                
                case CilFlowControl.ConditionalBranch:
                    return GetConditionalBranchTransitions(instruction, successorsBuffer);
                
                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    return 0;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int GetFallThroughTransitions(CilInstruction instruction, Span<SuccessorInfo> successorsBuffer)
        {
            // Fallthrough instructions always move to the next instruction.
            successorsBuffer[0] = FallThrough(instruction);
            return 1;
        }

        private static int GetUnconditionalBranchTransitions(CilInstruction instruction, Span<SuccessorInfo> successorsBuffer)
        {
            // Unconditional branches always move to the instruction referenced in the operand.
            var label = (ICilLabel) instruction.Operand!;
            successorsBuffer[0] = new SuccessorInfo(label.Offset, ControlFlowEdgeType.Unconditional);
            return 1;
        }

        private static int GetConditionalBranchTransitions(CilInstruction instruction, Span<SuccessorInfo> successorsBuffer)
        {
            // Conditional branches can reference one or more instructions in the operand.
            switch (instruction.Operand)
            {
                case ICilLabel singleTarget:
                    successorsBuffer[0] = Conditional(singleTarget);
                    successorsBuffer[1] = FallThrough(instruction);
                    return 2;

                case IList<ICilLabel> multipleTargets:
                    for (int i = 0; i < multipleTargets.Count; i++)
                        successorsBuffer[i] = Conditional(multipleTargets[i]);
                    successorsBuffer[multipleTargets.Count] = FallThrough(instruction);
                    return multipleTargets.Count + 1;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static SuccessorInfo FallThrough(CilInstruction instruction)
        {
            return new SuccessorInfo(instruction.Offset + instruction.Size, ControlFlowEdgeType.FallThrough);
        }

        private static SuccessorInfo Conditional(ICilLabel singleTarget)
        {
            return new SuccessorInfo(singleTarget.Offset, ControlFlowEdgeType.Conditional);
        }
    }
}