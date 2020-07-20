using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using JavaResolver.Class.Code;

namespace Echo.Platforms.JavaResolver
{
    /// <summary>
    ///     Provides an implementation of <see cref="IStaticSuccessorResolver{ByteCodeInstruction}"/>
    /// </summary>
    public class ByteCodeStaticSuccessorResolver  : IStaticSuccessorResolver<ByteCodeInstruction>
    {
        private readonly ByteCodeMethodBody _methodBody;
        
        /// <summary>
        ///     Creates a new bytecode staticSuccessorResolver.
        /// </summary>
        /// <param name="methodBody">The method body.</param>
        public ByteCodeStaticSuccessorResolver(ByteCodeMethodBody methodBody) => _methodBody = methodBody;
        
        /// <inheritdoc />
        public ICollection<SuccessorInfo> GetSuccessors(ByteCodeInstruction instruction)
        {
            switch (instruction.OpCode.FlowControl)
            {
                case ByteCodeFlowControl.Break:
                case ByteCodeFlowControl.Call:
                case ByteCodeFlowControl.Meta:
                case ByteCodeFlowControl.Next:
                    return new[] { GetFallThroughSuccessor(instruction) };
                
                case ByteCodeFlowControl.Branch:
                    return new[] { GetUnconditionalSuccessor(instruction) };
                
                case ByteCodeFlowControl.ConditionalBranch:
                    return GetConditionalSuccessors(instruction);
                
                case ByteCodeFlowControl.Return:
                case ByteCodeFlowControl.Throw:
                    return Array.Empty<SuccessorInfo>();
                
                default:
                    throw new NotSupportedException();
            }
        }
        
        /// <summary>
        ///     Creates a collection of Conditional successor.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns> The collection. </returns>
        private ICollection<SuccessorInfo> GetConditionalSuccessors(ByteCodeInstruction instruction)
        {
            var result = new List<SuccessorInfo>(1);
            switch (instruction.Operand)
            {
                case ByteCodeInstruction singleTarget:
                    result.Add(GetConditionalSuccessor(singleTarget));
                    result.Add(GetFallThroughSuccessor(instruction));
                    break;
                
                case TableSwitch multipleTargets:
                    result.AddRange(multipleTargets.GetOffsets()
                        .Select(offset => GetConditionalSuccessor(_methodBody.Instructions
                            .Single(x => x.Offset == offset))));
                    break;
            }
            
            return result;
        }

        /// <summary>
        ///     Creates a Fallthrough Successor.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns> the Fallthrough successor. </returns>
        private SuccessorInfo GetFallThroughSuccessor(ByteCodeInstruction instruction)
            => new SuccessorInfo(instruction.Offset + instruction.Size, ControlFlowEdgeType.FallThrough);
        
        /// <summary>
        ///     Creates a Conditional Successor.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns> the Conditional successor. </returns>
        private SuccessorInfo GetConditionalSuccessor(ByteCodeInstruction instruction)
            => new SuccessorInfo(instruction.Offset, ControlFlowEdgeType.Conditional);
        
        /// <summary>
        ///     Creates a Unconditional Successor.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns> the Unconditional successor. </returns>
        private SuccessorInfo GetUnconditionalSuccessor(ByteCodeInstruction instruction)
            => new SuccessorInfo(((ByteCodeInstruction)instruction.Operand).Offset, ControlFlowEdgeType.FallThrough);

    }
}