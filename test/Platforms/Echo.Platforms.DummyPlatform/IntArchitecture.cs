using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Platforms.DummyPlatform
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
        
        public long GetOffset(in int instruction) => instruction;

        public string GetMnemonic(in int instruction) => instruction.ToString();

        public int GetOperandCount(in int instruction) => 0;

        public object GetOperand(in int instruction, int index) => throw new ArgumentOutOfRangeException();

        public int GetSize(in int instruction) => 1;

        public byte[] GetOpCodeBytes(in int instruction) => new byte[]
        {
            (byte) (instruction & 0xFF)
        };

        public byte[] GetOperandBytes(in int instruction) => new byte[0];

        public byte[] GetInstructionBytes(in int instruction) => GetOpCodeBytes(instruction);
        
        public InstructionFlowControl GetFlowControl(in int instruction) => InstructionFlowControl.Fallthrough;

        public int GetStackPushCount(in int instruction) => 0;

        public int GetStackPopCount(in int instruction) => 0;

        public IEnumerable<IVariable> GetReadVariables(in int instruction) => new IVariable[0];

        public IEnumerable<IVariable> GetWrittenVariables(in int instruction) => new IVariable[0];
    }
}