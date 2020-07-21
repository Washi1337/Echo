using System;
using System.Collections.Generic;
using System.Linq;
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
        public ICollection<SuccessorInfo> GetSuccessors(Instruction instruction)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Break:
                case FlowControl.Call:
                case FlowControl.Meta:
                case FlowControl.Next:
                    return new[] {Next()};

                case FlowControl.Branch:
                    return new[] {Branch(false)};

                case FlowControl.Cond_Branch when instruction.OpCode.Code == Code.Switch:
                    return Switch().Append(Next()).ToArray();

                case FlowControl.Cond_Branch:
                    return new[] {Branch(true), Next()};

                case FlowControl.Return:
                case FlowControl.Throw:
                    return Array.Empty<SuccessorInfo>();

                case FlowControl.Phi:
                    throw new NotSupportedException("There are no known instructions with Phi control flow");

                default:
                    throw new ArgumentOutOfRangeException();
            }

            SuccessorInfo Next() =>
                new SuccessorInfo(instruction.Offset + instruction.GetSize(), ControlFlowEdgeType.FallThrough);

            SuccessorInfo Branch(bool conditional) =>
                new SuccessorInfo(((Instruction) instruction.Operand).Offset,
                    conditional ? ControlFlowEdgeType.Conditional : ControlFlowEdgeType.FallThrough);

            IEnumerable<SuccessorInfo> Switch() =>
                ((Instruction[]) instruction.Operand).Select(i => new SuccessorInfo(i.Offset, ControlFlowEdgeType.Conditional));
        }
    }
}