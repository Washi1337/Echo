using System;
using System.Collections.Generic;
using Echo.Code;
using Echo.ControlFlow.Construction;
using Echo.Platforms.DummyPlatform.ControlFlow;

namespace Echo.Platforms.DummyPlatform.Code
{
    public class DummyArchitecture : IArchitecture<DummyInstruction>
    {
        public static DummyArchitecture Instance
        {
            get;
        } = new();

        public IStaticSuccessorResolver<DummyInstruction> SuccessorResolver
        {
            get;
        } = new DummyStaticSuccessorResolver();
        
        public long GetOffset(in DummyInstruction instruction) => instruction.Offset;

        public int GetSize(in DummyInstruction instruction) => 1;

        public InstructionFlowControl GetFlowControl(in DummyInstruction instruction) =>
            instruction.OpCode switch
            {
                DummyOpCode.Jmp => InstructionFlowControl.CanBranch,
                DummyOpCode.JmpCond => InstructionFlowControl.CanBranch,
                DummyOpCode.Ret => InstructionFlowControl.IsTerminator,
                DummyOpCode.Switch => InstructionFlowControl.CanBranch,
                _ => InstructionFlowControl.Fallthrough
            };

        public int GetStackPushCount(in DummyInstruction instruction) => instruction.PushCount;

        public int GetStackPopCount(in DummyInstruction instruction) => instruction.PopCount;
        
        public void GetReadVariables(in DummyInstruction instruction, ICollection<IVariable> variablesBuffer)
        {
            if (instruction.OpCode == DummyOpCode.Get)
                variablesBuffer.Add((IVariable) instruction.Operands[0]);
        }

        public void GetWrittenVariables(in DummyInstruction instruction, ICollection<IVariable> variablesBuffer)
        {
            if (instruction.OpCode == DummyOpCode.Set)
                variablesBuffer.Add((IVariable) instruction.Operands[0]);
        }
    }
}