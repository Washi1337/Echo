using System;
using System.Collections.Generic;
using Echo.Code;

namespace Echo.Platforms.DummyPlatform
{
    public class IntArchitecture : IArchitecture<int>
    {
        public static IArchitecture<int> Instance
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
        
        public void GetReadVariables(in int instruction, ICollection<IVariable> variablesBuffer)
        {
        }

        public void GetWrittenVariables(in int instruction, ICollection<IVariable> variablesBuffer)
        {
        }
    }
}