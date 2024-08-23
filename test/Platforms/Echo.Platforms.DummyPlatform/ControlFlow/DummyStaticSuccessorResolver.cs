using System;
using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.Platforms.DummyPlatform.Code;

namespace Echo.Platforms.DummyPlatform.ControlFlow
{
    public class DummyStaticSuccessorResolver : IStaticSuccessorResolver<DummyInstruction>
    {
        public static DummyStaticSuccessorResolver Instance { get; } = new();
        
        public void GetSuccessors(in DummyInstruction instruction, IList<SuccessorInfo> successorsBuffer)
        {
            switch (instruction.OpCode)
            {
                case DummyOpCode.Op:
                case DummyOpCode.Push:
                case DummyOpCode.Pop:
                case DummyOpCode.Get:
                case DummyOpCode.Set:
                    successorsBuffer.Add(new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough));
                    break;
                
                case DummyOpCode.Jmp:
                    successorsBuffer.Add(new SuccessorInfo((long) instruction.Operands[0], ControlFlowEdgeType.Unconditional));
                    break;
                
                case DummyOpCode.JmpCond:
                    successorsBuffer.Add(new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough));
                    successorsBuffer.Add(new SuccessorInfo((long) instruction.Operands[0], ControlFlowEdgeType.Conditional));
                    break;
                
                case DummyOpCode.PushOffset:
                    successorsBuffer.Add(new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough));
                    successorsBuffer.Add(new SuccessorInfo((long) instruction.Operands[0], ControlFlowEdgeType.None));
                    break;
                
                case DummyOpCode.Switch:
                    long[] targets = (long[]) instruction.Operands[0];
                    for (int i = 0; i < targets.Length; i++)
                        successorsBuffer.Add(new SuccessorInfo(targets[i], ControlFlowEdgeType.Conditional));
                    successorsBuffer.Add(new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough));
                    break;
                
                case DummyOpCode.Ret:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}