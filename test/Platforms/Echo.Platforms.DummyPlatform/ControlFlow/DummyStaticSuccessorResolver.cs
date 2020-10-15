using System;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.Platforms.DummyPlatform.Code;

namespace Echo.Platforms.DummyPlatform.ControlFlow
{
    public class DummyStaticSuccessorResolver : IStaticSuccessorResolver<DummyInstruction>
    {
        /// <inheritdoc />
        public int GetSuccessorsCount(in DummyInstruction instruction)
        {
            switch (instruction.OpCode)
            {
                case DummyOpCode.Op:
                case DummyOpCode.Push:
                case DummyOpCode.Pop:
                case DummyOpCode.Get:
                case DummyOpCode.Set:
                case DummyOpCode.Jmp:
                    return 1;
                
                case DummyOpCode.JmpCond:
                    return 2;
                
                case DummyOpCode.Switch:
                    var targets = ((long[]) instruction.Operands[0]);
                    return targets.Length + 1;
                
                case DummyOpCode.Ret:
                    return 0;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public int GetSuccessors(in DummyInstruction instruction, Span<SuccessorInfo> successorBuffer)
        {
            switch (instruction.OpCode)
            {
                case DummyOpCode.Op:
                case DummyOpCode.Push:
                case DummyOpCode.Pop:
                case DummyOpCode.Get:
                case DummyOpCode.Set:
                    successorBuffer[0] = new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough);
                    return 1;
                
                case DummyOpCode.Jmp:
                    successorBuffer[0] = new SuccessorInfo((long) instruction.Operands[0], ControlFlowEdgeType.Unconditional);
                    return 1;
                
                case DummyOpCode.JmpCond:
                    successorBuffer[0] = new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough);
                    successorBuffer[1] = new SuccessorInfo((long) instruction.Operands[0], ControlFlowEdgeType.Conditional);
                    return 2;
                
                case DummyOpCode.Switch:
                    var targets = (long[]) instruction.Operands[0];
                    for (int i = 0; i < targets.Length; i++)
                        successorBuffer[i] = new SuccessorInfo(targets[i], ControlFlowEdgeType.Conditional);
                    successorBuffer[targets.Length] = new SuccessorInfo(instruction.Offset + 1, ControlFlowEdgeType.FallThrough);
                    return targets.Length + 1;
                
                case DummyOpCode.Ret:
                    return 0;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}