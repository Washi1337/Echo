using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Construction.Static;
using Echo.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;

namespace Echo.Platforms.DummyPlatform.Code
{
    public class DummyArchitecture : IArchitecture<DummyInstruction>
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

        public int GetReadVariablesCount(in DummyInstruction instruction)
        {
            return instruction.OpCode == DummyOpCode.Get 
                ? 1
                : 0;
        }

        public int GetReadVariables(in DummyInstruction instruction, Span<IVariable> variablesBuffer)
        {
            if (instruction.OpCode == DummyOpCode.Get)
            {
                variablesBuffer[0] = (IVariable) instruction.Operands[0];
                return 1;
            }

            return 0;
        }

        public int GetWrittenVariablesCount(in DummyInstruction instruction)
        {
            return instruction.OpCode == DummyOpCode.Set 
                ? 1
                : 0;
        }

        public int GetWrittenVariables(in DummyInstruction instruction, Span<IVariable> variablesBuffer)
        {
            if (instruction.OpCode == DummyOpCode.Set)
            {
                variablesBuffer[0] = (IVariable) instruction.Operands[0];
                return 1;
            }

            return 0;
        }
    }
}