using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;

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
        public int GetSuccessorsCount(in Instruction instruction, GraphBuilderContext<Instruction> context)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Break:
                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                case FlowControl.Branch:
                    return 1;

                case FlowControl.Cond_Branch when instruction.OpCode.Code == Code.Switch:
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
        public int GetSuccessors(in Instruction instruction, Span<SuccessorInfo> successorsBuffer,
            GraphBuilderContext<Instruction> context)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Break:
                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                    successorsBuffer[0] = Next(instruction);
                    return 1;

                case FlowControl.Branch:
                    successorsBuffer[0] = Branch(false, instruction);
                    return 1;

                case FlowControl.Cond_Branch when instruction.OpCode.Code == Code.Switch:
                    var multipleTargets = (Instruction[]) instruction.Operand;
                    for (int i = 0; i < multipleTargets.Length; i++)
                        successorsBuffer[i] = new SuccessorInfo(multipleTargets[i].Offset, ControlFlowEdgeType.Conditional);
                    successorsBuffer[multipleTargets.Length] = Next(instruction);
                    return multipleTargets.Length + 1;

                case FlowControl.Cond_Branch:
                    successorsBuffer[0] = Branch(true, instruction);
                    successorsBuffer[1] = Next(instruction);
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

        private static SuccessorInfo Next(Instruction instruction)
        {
            return new SuccessorInfo(instruction.Offset + instruction.GetSize(), ControlFlowEdgeType.FallThrough);
        }

        private static SuccessorInfo Branch(bool conditional, Instruction instruction) =>
            new SuccessorInfo(((Instruction) instruction.Operand).Offset, conditional
                ? ControlFlowEdgeType.Conditional
                : ControlFlowEdgeType.FallThrough);
    }
}