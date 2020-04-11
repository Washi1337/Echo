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
        public ICollection<SuccessorInfo> GetSuccessors(CilInstruction instruction)
        {
            // Multiplex based on flow control.
            
            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Break:
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                    return GetFallThroughTransitions(instruction);
                
                case CilFlowControl.Branch:
                    return GetUnconditionalBranchTransitions(instruction);
                
                case CilFlowControl.ConditionalBranch:
                    return GetConditionalBranchTransitions(instruction);
                
                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    return Array.Empty<SuccessorInfo>();
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ICollection<SuccessorInfo> GetFallThroughTransitions(CilInstruction instruction)
        {
            // Fallthrough instructions always move to the next instruction.
            
            return new[]
            {
                CreateFallThroughTransition(instruction)
            };
        }

        private static ICollection<SuccessorInfo> GetUnconditionalBranchTransitions(CilInstruction instruction)
        {
            // Unconditional branches always move to the instruction referenced in the operand.
            
            var label = (ICilLabel) instruction.Operand;
            return new[]
            {
                new SuccessorInfo(label.Offset, ControlFlowEdgeType.FallThrough)
            };
        }

        private static ICollection<SuccessorInfo> GetConditionalBranchTransitions(CilInstruction instruction)
        {
            // Conditional branches can reference one or more instructions in the operand.
            
            var result = new List<SuccessorInfo>(1);
            switch (instruction.Operand)
            {
                case ICilLabel singleTarget:
                    result.Add(CreateConditionalTransition(singleTarget));
                    break;

                case IEnumerable<ICilLabel> multipleTargets:
                    foreach (var target in multipleTargets)
                        result.Add(CreateConditionalTransition(target));
                    break;
            }

            // Add the successor that transfers control to the next instruction (= false edge).
            result.Add(CreateFallThroughTransition(instruction));
            return result;
        }

        private static SuccessorInfo CreateFallThroughTransition(CilInstruction instruction)
        {
            return new SuccessorInfo(instruction.Offset + instruction.Size, ControlFlowEdgeType.FallThrough);
        }

        private static SuccessorInfo CreateConditionalTransition(ICilLabel singleTarget)
        {
            return new SuccessorInfo(singleTarget.Offset, ControlFlowEdgeType.Conditional);
        }
    }
}