using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Platforms.DummyPlatform.Code
{
    public class DummyInstruction : IInstruction
    {
        private int _pushCount;
        private int _popCount;

        public DummyInstruction(long offset, int popCount, int pushCount)
        {
            Offset = offset;
            Mnemonic = "op";
            Operand = new object[0];
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
    }
}