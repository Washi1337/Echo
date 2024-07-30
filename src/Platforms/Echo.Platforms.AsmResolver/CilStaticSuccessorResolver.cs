using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;

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
        public static CilStaticSuccessorResolver Instance { get; } = new();
        
        /// <inheritdoc />
        public void GetSuccessors(in CilInstruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            // Multiplex based on flow control.
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Break or CilFlowControl.Meta or CilFlowControl.Next or CilFlowControl.Call:
                    if (instruction.OpCode.Code != CilCode.Jmp)
                        AddFallThrough(instruction, successorsBuffer);
                    break;
                
                case CilFlowControl.Branch:
                    AddUnconditionalBranch(instruction, successorsBuffer);
                    break;
                
                case CilFlowControl.ConditionalBranch:
                    AddConditionalBranches(instruction, successorsBuffer);
                    break;

                case CilFlowControl.Return or CilFlowControl.Throw:
                    return;

                case CilFlowControl.Phi:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void AddFallThrough(CilInstruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            successorsBuffer.Add(FallThrough(instruction));
        }

        private static void AddUnconditionalBranch(CilInstruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            // Unconditional branches always move to the instruction referenced in the operand.
            var label = (ICilLabel) instruction.Operand!;
            successorsBuffer.Add(new SuccessorInfo(label.Offset, ControlFlowEdgeType.Unconditional));
        }

        private static void AddConditionalBranches(CilInstruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            // Conditional branches can reference one or more instructions in the operand.
            switch (instruction.Operand)
            {
                case ICilLabel singleTarget:
                    successorsBuffer.Add(Conditional(singleTarget));
                    successorsBuffer.Add(FallThrough(instruction));
                    break;

                case IList<ICilLabel> multipleTargets:
                    for (int i = 0; i < multipleTargets.Count; i++)
                        successorsBuffer.Add(Conditional(multipleTargets[i]));
                    successorsBuffer.Add(FallThrough(instruction));
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static SuccessorInfo Conditional(ICilLabel singleTarget)
        {
            return new SuccessorInfo(singleTarget.Offset, ControlFlowEdgeType.Conditional);
        }

        private static SuccessorInfo FallThrough(CilInstruction instruction)
        {
            return new SuccessorInfo(instruction.Offset + instruction.Size, ControlFlowEdgeType.FallThrough);
        }
    }
}