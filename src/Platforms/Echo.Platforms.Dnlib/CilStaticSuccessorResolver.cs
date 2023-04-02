using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using DnlibCode = dnlib.DotNet.Emit.Code;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Provides an implementation of <see cref="IStaticSuccessorResolver{CilInstruction}"/>
    /// </summary>
    public class CilStaticSuccessorResolver : IStaticSuccessorResolver<Instruction>
    {
        /// <summary>
        /// Gets a reusable singleton instance of the static successor resolver for the CIL architecture.
        /// </summary>
        public static CilStaticSuccessorResolver Instance
        {
            get;
        } = new CilStaticSuccessorResolver();

        /// <inheritdoc />
        public int GetSuccessorsCount(in Instruction instruction)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Break:
                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                case FlowControl.Branch:
                    return 1;

                case FlowControl.Cond_Branch when instruction.OpCode.Code == DnlibCode.Switch:
                    return ((ICollection<Instruction>) instruction.Operand).Count + 1;

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
        public int GetSuccessors(in Instruction instruction, Span<SuccessorInfo> successorsBuffer)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Break:
                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                    successorsBuffer[0] = FallThrough(instruction);
                    return 1;

                case FlowControl.Branch:
                    successorsBuffer[0] = Branch(false, instruction);
                    return 1;

                case FlowControl.Cond_Branch when instruction.OpCode.Code == DnlibCode.Switch:
                    var multipleTargets = (Instruction[]) instruction.Operand;
                    for (int i = 0; i < multipleTargets.Length; i++)
                        successorsBuffer[i] = new SuccessorInfo(multipleTargets[i].Offset, ControlFlowEdgeType.Conditional);
                    successorsBuffer[multipleTargets.Length] = FallThrough(instruction);
                    return multipleTargets.Length + 1;

                case FlowControl.Cond_Branch:
                    successorsBuffer[0] = Branch(true, instruction);
                    successorsBuffer[1] = FallThrough(instruction);
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

        private static SuccessorInfo FallThrough(Instruction instruction)
        {
            return new SuccessorInfo(instruction.Offset + instruction.GetSize(), ControlFlowEdgeType.FallThrough);
        }

        private static SuccessorInfo Branch(bool conditional, Instruction instruction) =>
            new SuccessorInfo(((Instruction) instruction.Operand).Offset, conditional
                ? ControlFlowEdgeType.Conditional
                : ControlFlowEdgeType.Unconditional);
    }
}