using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Construction.Static;
using Echo.Core.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;

namespace Echo.Platforms.DummyPlatform.Code
{
    public class DummyArchitecture : IInstructionSetArchitecture<DummyInstruction>
    {
        public static DummyArchitecture Instance
        {
            get;
        } = new DummyArchitecture();

        public IStaticSuccessorResolver<DummyInstruction> SuccessorResolver
        {
            get;
        } = new DummyStaticSuccessorResolver();
        
        public long GetOffset(in DummyInstruction instruction) => instruction.Offset;

        public string GetMnemonic(in DummyInstruction instruction) => instruction.Mnemonic;

        public int GetOperandCount(in DummyInstruction instruction) => instruction.Operands.Count;

        public object GetOperand(in DummyInstruction instruction, int index) => instruction.Operands[index];

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

        public int GetReadVariablesCount(in DummyInstruction instruction) => 0;

        public int GetReadVariables(in DummyInstruction instruction, Span<IVariable> variablesBuffer) => 0;

        public int GetWrittenVariablesCount(in DummyInstruction instruction) => 0;

        public int GetWrittenVariables(in DummyInstruction instruction, Span<IVariable> variablesBuffer) => 0;

        public IEnumerable<IVariable> GetReadVariables(in DummyInstruction instruction) =>
            Enumerable.Empty<IVariable>();

        public IEnumerable<IVariable> GetWrittenVariables(in DummyInstruction instruction) =>
            Enumerable.Empty<IVariable>();
    }
}