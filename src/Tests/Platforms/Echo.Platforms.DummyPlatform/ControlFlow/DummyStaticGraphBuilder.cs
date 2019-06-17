using System;
using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.Core.Code;
using Echo.Platforms.DummyPlatform.Code;

namespace Echo.Platforms.DummyPlatform.ControlFlow
{
    public class DummyStaticGraphBuilder : StaticGraphBuilder<DummyInstruction>
    {
        public DummyStaticGraphBuilder(IInstructionProvider<DummyInstruction> provider) 
            : base(provider)
        {
        }

        protected override ICollection<SuccessorInfo> GetSuccessors(DummyInstruction instruction)
        {
            var result = new List<SuccessorInfo>(1);
            switch (instruction.OpCode)
            {
                case DummyOpCode.Op:
                    result.Add(new SuccessorInfo(instruction.Offset + instruction.Size, EdgeType.FallThrough));
                    break;
                case DummyOpCode.Jmp:
                    result.Add(new SuccessorInfo((long) instruction.Operand[0], EdgeType.FallThrough));
                    break;
                case DummyOpCode.JmpCond:
                    result.Add(new SuccessorInfo(instruction.Offset + instruction.Size, EdgeType.FallThrough));
                    result.Add(new SuccessorInfo((long) instruction.Operand[0], EdgeType.Conditional));
                    break;
                case DummyOpCode.Ret:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
        
    }
}