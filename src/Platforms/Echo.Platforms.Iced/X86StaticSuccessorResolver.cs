using System;
using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
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
        public ICollection<SuccessorInfo> GetSuccessors(Instruction instruction)
        {
            switch (instruction.FlowControl)
            {
                case FlowControl.UnconditionalBranch:
                    return GetUnconditionalBranchSuccessors(instruction);

                case FlowControl.ConditionalBranch:
                    return GetConditionalBranchSuccessors(instruction);
                
                case FlowControl.IndirectBranch: 
                case FlowControl.Return:
                    return Array.Empty<SuccessorInfo>();
                
                default:
                    return GetFallthroughSuccessors(instruction);
            }
        }

        private static ICollection<SuccessorInfo> GetUnconditionalBranchSuccessors(Instruction instruction)
        {
            return new[]
            {
                new SuccessorInfo((long) instruction.NearBranchTarget, ControlFlowEdgeType.FallThrough),
            };
        }

        private static ICollection<SuccessorInfo> GetConditionalBranchSuccessors(Instruction instruction)
        {
            return new[]
            {
                new SuccessorInfo((long) instruction.NearBranchTarget, ControlFlowEdgeType.Conditional),
                new SuccessorInfo((long) instruction.IP + instruction.Length, ControlFlowEdgeType.FallThrough)
            };
        }

        private static ICollection<SuccessorInfo> GetFallthroughSuccessors(Instruction instruction)
        {
            return new[]
            {
                new SuccessorInfo((long) instruction.IP + instruction.Length, ControlFlowEdgeType.FallThrough)
            };
        }
        
    }
}