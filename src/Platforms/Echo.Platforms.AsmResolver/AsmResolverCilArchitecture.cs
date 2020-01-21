using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides a description of the CIL instruction set architecture (ISA) that is modelled by AsmResolver.   
    /// </summary>
    public class AsmResolverCilArchitecture : IInstructionSetArchitecture<CilInstruction>
    {
        /// <summary>
        /// Gets a singleton instance of this architecture description.
        /// </summary>
        public static AsmResolverCilArchitecture Instance
        {
            get;
        } = new AsmResolverCilArchitecture();

        /// <summary>
        /// Gets the default static successor resolution engine for this architecture.
        /// </summary>
        public AsmResolverCilSuccessorResolver SuccessorResolver
        {
            get;
        } = new AsmResolverCilSuccessorResolver();

        /// <inheritdoc />
        public long GetOffset(CilInstruction instruction) =>
            instruction.Offset;

        /// <inheritdoc />
        public string GetMnemonic(CilInstruction instruction) =>
            instruction.OpCode.Mnemonic;

        /// <inheritdoc />
        public int GetOperandCount(CilInstruction instruction) =>
            instruction.Operand == null ? 0 : 1;

        /// <inheritdoc />
        public object GetOperand(CilInstruction instruction, int index) =>
            instruction.Operand != null && index == 0 ? instruction.Operand : throw new IndexOutOfRangeException();

        /// <inheritdoc />
        public int GetSize(CilInstruction instruction) =>
            instruction.Size;

        /// <inheritdoc />
        public byte[] GetOpCodeBytes(CilInstruction instruction)
        {
            var opCode = instruction.OpCode;
            return opCode.Size == 2
                ? new[] { opCode.Byte1, opCode.Byte2 }
                : new[] { opCode.Byte1 };
        }

        /// <inheritdoc />
        public byte[] GetOperandBytes(CilInstruction instruction)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public byte[] GetInstructionBytes(CilInstruction instruction)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int GetStackPushCount(CilInstruction instruction)
        {
            return instruction.GetStackPushCount();
        }

        /// <inheritdoc />
        public int GetStackPopCount(CilInstruction instruction)
        {
            // TODO: incorporate the parent method body somehow.
            return instruction.GetStackPopCount(null);
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetReadVariables(CilInstruction instruction)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetWrittenVariables(CilInstruction instruction)
        {
            throw new NotImplementedException();
        }
    }
}