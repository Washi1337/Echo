using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;

namespace Echo.Platforms.AsmResolver
{
    public class CilStaticSuccessorResolver : IStaticSuccessorResolver<CilInstruction>
    {
        public static CilStaticSuccessorResolver Instance
        {
            get;
        } = new CilStaticSuccessorResolver();
        
        public ICollection<SuccessorInfo> GetSuccessors(CilInstruction instruction)
        {
            var result = new List<SuccessorInfo>(1);

            switch (instruction.OpCode.FlowControl)
            {
                case CilFlowControl.Break:
                case CilFlowControl.Call:
                case CilFlowControl.Meta:
                case CilFlowControl.Next:
                    result.Add(CreateFallThrough(instruction));
                    break;
                
                case CilFlowControl.Branch:
                    var label = (ICilLabel) instruction.Operand;
                    result.Add(new SuccessorInfo(label.Offset, EdgeType.FallThrough));
                    break;
                
                case CilFlowControl.ConditionalBranch:
                    switch (instruction.Operand)
                    {
                        case ICilLabel singleTarget:
                            result.Add(CreateConditional(singleTarget));
                            break;
                        
                        case IEnumerable<ICilLabel> multipleTargets:
                            foreach (var target in multipleTargets)
                                result.Add(CreateConditional(target));
                            break;
                    }

                    result.Add(CreateFallThrough(instruction));
                    break;
                
                case CilFlowControl.Phi:
                    throw new NotSupportedException();
                
                case CilFlowControl.Return:
                case CilFlowControl.Throw:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return result;
        }

        private static SuccessorInfo CreateFallThrough(CilInstruction instruction)
        {
            return new SuccessorInfo(instruction.Offset + instruction.Size, EdgeType.FallThrough);
        }

        private static SuccessorInfo CreateConditional(ICilLabel singleTarget)
        {
            return new SuccessorInfo(singleTarget.Offset, EdgeType.Conditional);
        }
    }
}