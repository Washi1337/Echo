using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Platforms.DummyPlatform.Code
{
    public class DummyInstruction
    {
        public static DummyInstruction Op(long offset, int popCount, int pushCount)
        {
            return new DummyInstruction(offset, DummyOpCode.Op, popCount, pushCount, popCount, pushCount);
        }
        
        public static DummyInstruction Push(long offset, int pushCount)
        {
            return new DummyInstruction(offset, DummyOpCode.Push, 0, pushCount, 0, pushCount);
        }
        
        public static DummyInstruction Pop(long offset, int popCount)
        {
            return new DummyInstruction(offset, DummyOpCode.Pop, popCount, 0, popCount, 0);
        }

        public static DummyInstruction Jmp(long offset, long target)
        {
            return new DummyInstruction(offset, DummyOpCode.Jmp, 0, 0, target);
        }

        public static DummyInstruction JmpCond(long offset, long target)
        {
            return new DummyInstruction(offset, DummyOpCode.JmpCond, 1, 0, target);
        }

        public static DummyInstruction Switch(long offset, params long[] targets)
        {
            return new DummyInstruction(offset, DummyOpCode.Switch, 1, 0, targets);
        }

        public static DummyInstruction Ret(long offset)
        {
            return new DummyInstruction(offset, DummyOpCode.Ret, 0, 0);
        }

        public DummyInstruction(long offset, DummyOpCode opCode, int popCount, int pushCount, params object[] operands)
        {
            Offset = offset;
            OpCode = opCode;
            Operands = operands;
            PopCount = popCount;
            PushCount = pushCount;
        }

        public long Offset
        {
            get;
        }

        public DummyOpCode OpCode
        {
            get;
        }

        public string Mnemonic => OpCode.ToString().ToLowerInvariant();

        public IList<object> Operands
        {
            get;
        }

        public int PushCount
        {
            get;
        }

        public int PopCount
        {
            get;
        }

        public override string ToString()
        {
            return $"Label_{Offset:X4}: {Mnemonic}({string.Join(", ", Operands)})";
        }
    }
}