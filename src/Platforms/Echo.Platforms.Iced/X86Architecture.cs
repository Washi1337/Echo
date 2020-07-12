using System;
using System.Collections.Generic;
using Echo.Core.Code;
using Iced.Intel;

namespace Echo.Platforms.Iced
{
    /// <summary>
    /// Provides a description of the x86 instruction set architecture (ISA) that is modelled by Iced.   
    /// </summary>
    public class X86Architecture : IInstructionSetArchitecture<Instruction>
    {
        private readonly Formatter _formatter = new NasmFormatter();
        private readonly InstructionInfoFactory _infoFactory = new InstructionInfoFactory();
        private readonly IDictionary<Register, X86RegisterVariable> _registers = new Dictionary<Register, X86RegisterVariable>();

        public X86Architecture()
        {
            foreach (Register register in Enum.GetValues(typeof(Register)))
                _registers[register] = new X86RegisterVariable(register);
        }
        
        /// <inheritdoc />
        public long GetOffset(Instruction instruction) => (long) instruction.IP;

        /// <inheritdoc />
        public string GetMnemonic(Instruction instruction)
        {
            var output = new StringOutput();
            _formatter.FormatMnemonic(instruction, output);
            return output.ToString();
        }

        /// <inheritdoc />
        public int GetOperandCount(Instruction instruction) => instruction.OpCode.OpCount;

        /// <inheritdoc />
        public object GetOperand(Instruction instruction, int index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int GetSize(Instruction instruction) => instruction.Length;

        /// <inheritdoc />
        public byte[] GetOpCodeBytes(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public byte[] GetOperandBytes(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public byte[] GetInstructionBytes(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public InstructionFlowControl GetFlowControl(Instruction instruction)
        {
            switch (instruction.FlowControl)
            {
                case FlowControl.UnconditionalBranch:
                case FlowControl.IndirectBranch:
                    return InstructionFlowControl.CanBranch;
                
                case FlowControl.ConditionalBranch:
                    return InstructionFlowControl.CanBranch | InstructionFlowControl.Fallthrough;
                
                case FlowControl.Return:
                    return InstructionFlowControl.IsTerminator;
                
                default:
                    return InstructionFlowControl.Fallthrough;
            }
        }

        /// <inheritdoc />
        public int GetStackPushCount(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int GetStackPopCount(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetReadVariables(Instruction instruction)
        {
            IList<IVariable> result = null;
            
            ref readonly var info = ref _infoFactory.GetInfo(instruction);
            foreach (var use in info.GetUsedRegisters())
            {
                switch (use.Access)
                {
                    case OpAccess.Read:
                    case OpAccess.CondRead:
                    case OpAccess.ReadWrite:
                    case OpAccess.ReadCondWrite:
                        result ??= new List<IVariable>();
                        result.Add(_registers[use.Register]);
                        break;
                }
            }

            return result ?? Array.Empty<IVariable>();
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetWrittenVariables(Instruction instruction)
        {
            IList<IVariable> result = null;
            
            ref readonly var info = ref _infoFactory.GetInfo(instruction);
            foreach (var use in info.GetUsedRegisters())
            {
                switch (use.Access)
                {
                    case OpAccess.Write:
                    case OpAccess.CondWrite:
                    case OpAccess.ReadWrite:
                    case OpAccess.ReadCondWrite:
                        result ??= new List<IVariable>();
                        result.Add(_registers[use.Register]);
                        break;
                }
            }

            return result ?? Array.Empty<IVariable>();
        }
        
    }
}