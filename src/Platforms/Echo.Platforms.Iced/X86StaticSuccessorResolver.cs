using System;
using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Iced.Intel;

namespace Echo.Platforms.Iced
{
    /// <summary>
    /// Provides an implementation for the <see cref="IStaticSuccessorResolver{TInstruction}"/> that is able to
    /// obtain successor information of x86 instructions modelled by the <see cref="Instruction"/> structure.
    /// </summary>
    public class X86StaticSuccessorResolver : IStaticSuccessorResolver<Instruction>
    {
        /// <inheritdoc />
        public void GetSuccessors(in Instruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            switch (instruction.FlowControl)
            {
                case FlowControl.UnconditionalBranch:
                    UnconditionalBranch(instruction, successorsBuffer);
                    break;
                
                case FlowControl.ConditionalBranch:
                    ConditionalBranch(instruction, successorsBuffer);
                    break;
                
                case FlowControl.IndirectBranch: 
                case FlowControl.Return:
                    break;
                
                default:
                    FallThrough(instruction, successorsBuffer);
                    break;
            }
        }

        private static void UnconditionalBranch(Instruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            successorsBuffer.Add(new SuccessorInfo(
                (long) instruction.NearBranchTarget, 
                ControlFlowEdgeType.Unconditional
            ));
        }

        private static void ConditionalBranch(Instruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            successorsBuffer.Add(new SuccessorInfo(
                (long) instruction.NearBranchTarget, 
                ControlFlowEdgeType.Conditional
            ));
            
            successorsBuffer.Add(new SuccessorInfo(
                (long) instruction.IP + instruction.Length, 
                ControlFlowEdgeType.FallThrough
            ));
        }

        private static void FallThrough(Instruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            successorsBuffer.Add(new SuccessorInfo(
                (long) instruction.IP + instruction.Length,
                ControlFlowEdgeType.FallThrough
            ));
        }
    }
}