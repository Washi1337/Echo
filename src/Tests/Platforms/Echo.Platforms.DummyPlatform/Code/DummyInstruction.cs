using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Platforms.DummyPlatform.Code
{
    public class DummyInstruction : IInstruction
    {
        private int _pushCount;
        private int _popCount;

        public static DummyInstruction Op(long offset, int popCount, int pushCount)
        {
            return new DummyInstruction(offset, DummyOpCode.Op, popCount, pushCount, popCount, pushCount);
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
            _popCount = popCount;
            _pushCount = pushCount;
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

        public int Size => 1;

        public IEnumerable<byte> GetOpCodeBytes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<byte> GetOperandBytes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<byte> GetBytes()
        {
            throw new NotImplementedException();
        }
        
        public int GetStackPushCount()
        {
            return _pushCount;
        }

        public int GetStackPopCount()
        {
            return _popCount;
        }

        public IEnumerable<IVariable> GetReadVariables()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IVariable> GetWrittenVariables()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"Label_{Offset:X4}: {Mnemonic}({string.Join(", ", Operands)})";
        }
    }
}