using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Echo.Core.Code;

namespace Echo.ControlFlow.Tests
{
    public class IntArchitecture : IInstructionSetArchitecture<int>
    {
        public static IInstructionSetArchitecture<int> Instance
        {
            get;
        } = new IntArchitecture();

        private IntArchitecture()
        {
        }
        
        public long GetOffset(int instruction) => instruction;

        public string GetMnemonic(int instruction) => instruction.ToString();

        public int GetOperandCount(int instruction) => 0;

        public object GetOperand(int instruction, int index) => throw new ArgumentOutOfRangeException();

        public int GetSize(int instruction) => 1;

        public byte[] GetOpCodeBytes(int instruction) => new byte[]
        {
            (byte) (instruction & 0xFF)
        };

        public byte[] GetOperandBytes(int instruction) => new byte[0];

        public byte[] GetInstructionBytes(int instruction) => GetOpCodeBytes(instruction);

        public int GetStackPushCount(int instruction) => 0;

        public int GetStackPopCount(int instruction) => 0;

        public IEnumerable<IVariable> GetReadVariables(int instruction) => ImmutableArray<IVariable>.Empty;

        public IEnumerable<IVariable> GetWrittenVariables(int instruction) => ImmutableArray<IVariable>.Empty;
    }
}