using System;
using System.Collections.Generic;
using Echo.Core.Code;
using Echo.Core.Emulation;
using Echo.Core.Values;

namespace Echo.Symbolic.Tests
{
    public class DummyInstruction : IInstruction
    {
        private int _pushCount;
        private int _popCount;

        public DummyInstruction(long offset, int popCount, int pushCount)
        {
            Offset = offset;
            Mnemonic = "op";
            Operand = Array.Empty<object>();
            _popCount = popCount;
            _pushCount = pushCount;
        }
        
        public long Offset
        {
            get;
        }

        public string Mnemonic
        {
            get;
        }

        public IList<object> Operand
        {
            get;
        }

        public int Size => 1;

        public IEnumerable<byte> GetOpCodeBytes()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<byte> GetOperandBytes()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<byte> GetBytes()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<long> GetSuccessors<TValue>(IProgramState<TValue> state) where TValue : IValue
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public IEnumerable<IVariable> GetWrittenVariables()
        {
            throw new System.NotImplementedException();
        }
    }
}