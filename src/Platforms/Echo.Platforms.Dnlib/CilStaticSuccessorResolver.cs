using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using DnlibCode = dnlib.DotNet.Emit.Code;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Provides an implementation of <see cref="IStaticSuccessorResolver{Instruction}"/>
    /// </summary>
    public class CilStaticSuccessorResolver : IStaticSuccessorResolver<Instruction>
    {
        /// <summary>
        /// Gets a reusable singleton instance of the static successor resolver for the CIL architecture.
        /// </summary>
        public static CilStaticSuccessorResolver Instance { get; } = new();
        
        /// <inheritdoc />
        public void GetSuccessors(in Instruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            // Multiplex based on flow control.
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Break or FlowControl.Meta or FlowControl.Next or FlowControl.Call:
                    if (instruction.OpCode.Code != DnlibCode.Jmp)
                        AddFallThrough(instruction, successorsBuffer);
                    break;
                
                case FlowControl.Branch:
                    AddUnconditionalBranch(instruction, successorsBuffer);
                    break;
                
                case FlowControl.Cond_Branch:
                    AddConditionalBranches(instruction, successorsBuffer);
                    break;

                case FlowControl.Return or FlowControl.Throw:
                    return;

                case FlowControl.Phi:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void AddFallThrough(Instruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            successorsBuffer.Add(FallThrough(instruction));
        }

        private static void AddUnconditionalBranch(Instruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            // Unconditional branches always move to the instruction referenced in the operand.
            var label = (Instruction) instruction.Operand!;
            successorsBuffer.Add(new SuccessorInfo(label.Offset, ControlFlowEdgeType.Unconditional));
        }

        private static void AddConditionalBranches(Instruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            // Conditional branches can reference one or more instructions in the operand.
            switch (instruction.Operand)
            {
                case Instruction singleTarget:
                    successorsBuffer.Add(Conditional(singleTarget));
                    successorsBuffer.Add(FallThrough(instruction));
                    break;

                case IList<Instruction> multipleTargets:
                    for (int i = 0; i < multipleTargets.Count; i++)
                        successorsBuffer.Add(Conditional(multipleTargets[i]));
                    successorsBuffer.Add(FallThrough(instruction));
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static SuccessorInfo Conditional(Instruction singleTarget)
        {
            return new SuccessorInfo(singleTarget.Offset, ControlFlowEdgeType.Conditional);
        }

        private static SuccessorInfo FallThrough(Instruction instruction)
        {
            return new SuccessorInfo(instruction.Offset + instruction.GetSize(), ControlFlowEdgeType.FallThrough);
        }
    }
}