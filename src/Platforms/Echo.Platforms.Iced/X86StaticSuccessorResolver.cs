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
        public int GetSuccessorsCount(in Instruction instruction, GraphBuilderContext<Instruction> context)
        {
            switch (instruction.FlowControl)
            {
                case FlowControl.ConditionalBranch:
                    return 2;
                
                case FlowControl.IndirectBranch: 
                case FlowControl.Return:
                    return 0;
                
                default:
                    return 1;
            }
        }

        /// <inheritdoc />
        public int GetSuccessors(in Instruction instruction, Span<SuccessorInfo> successorsBuffer,
            GraphBuilderContext<Instruction> context)
        {
            switch (instruction.FlowControl)
            {
                case FlowControl.UnconditionalBranch:
                    return GetUnconditionalBranchSuccessors(instruction, successorsBuffer);

                case FlowControl.ConditionalBranch:
                    return GetConditionalBranchSuccessors(instruction, successorsBuffer);
                
                case FlowControl.IndirectBranch: 
                case FlowControl.Return:
                    return 0;
                
                default:
                    return GetFallthroughSuccessors(instruction, successorsBuffer);
            }
        }

        private static int GetUnconditionalBranchSuccessors(Instruction instruction, Span<SuccessorInfo> successorsBuffer)
        {
            successorsBuffer[0] = new SuccessorInfo(
                (long) instruction.NearBranchTarget, 
                ControlFlowEdgeType.FallThrough);
            
            return 1;
        }

        private static int GetConditionalBranchSuccessors(Instruction instruction, Span<SuccessorInfo> successorsBuffer)
        {
            successorsBuffer[0] = new SuccessorInfo(
                (long) instruction.NearBranchTarget, 
                ControlFlowEdgeType.Conditional);
            
            successorsBuffer[1] =new SuccessorInfo(
                (long) instruction.IP + instruction.Length, 
                ControlFlowEdgeType.FallThrough);
            
            return 2;
        }

        private static int GetFallthroughSuccessors(Instruction instruction, Span<SuccessorInfo> successorsBuffer)
        {
            successorsBuffer[0] = new SuccessorInfo(
                (long) instruction.IP + instruction.Length,
                ControlFlowEdgeType.FallThrough);
            
            return 1;
        }
        
    }
}