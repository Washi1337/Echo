using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.Core.Code;
using Echo.Platforms.DummyPlatform.Code;

namespace Echo.Platforms.DummyPlatform.ControlFlow
{
    public class DummyStaticSuccessorResolver : IStaticSuccessorResolver<DummyInstruction>
    {
        
        public ICollection<SuccessorInfo> GetSuccessors(DummyInstruction instruction)
        {
            var result = new List<SuccessorInfo>(1);
            switch (instruction.OpCode)
            {
                case DummyOpCode.Op:
                    result.Add(new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough));
                    break;
                case DummyOpCode.Jmp:
                    result.Add(new SuccessorInfo((long) instruction.Operands[0], ControlFlowEdgeType.FallThrough));
                    break;
                case DummyOpCode.JmpCond:
                    result.Add(new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough));
                    result.Add(new SuccessorInfo((long) instruction.Operands[0], ControlFlowEdgeType.Conditional));
                    break;
                case DummyOpCode.Switch:
                    result.Add(new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough));
                    result.AddRange(((long[]) instruction.Operands[0])
                        .Select(target => new SuccessorInfo(target, ControlFlowEdgeType.Conditional)));
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