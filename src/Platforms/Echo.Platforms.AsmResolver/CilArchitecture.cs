using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides a description of the CIL instruction set architecture (ISA) that is modelled by AsmResolver.   
    /// </summary>
    public class CilArchitecture : IInstructionSetArchitecture<CilInstruction>
    {
        private readonly CilMethodBody _parentBody;
        private readonly CilVariable[] _variables;
        private readonly CilParameter[] _parameters;

        /// <summary>
        /// Creates a new CIL architecture description based on a CIL method body.
        /// </summary>
        /// <param name="parentBody">The method body.</param>
        public CilArchitecture(CilMethodBody parentBody)
        {
            _parentBody = parentBody ?? throw new ArgumentNullException(nameof(parentBody));
            
            _variables = parentBody.LocalVariables
                .Select(v => new CilVariable(v))
                .ToArray();
            
            _parameters = parentBody.Owner.Parameters
                .Select(p => new CilParameter(p))
                .ToArray();
        }

        /// <summary>
        /// Gets the default static successor resolution engine for this architecture.
        /// </summary>
        public CilStaticSuccessorResolver SuccessorResolver
        {
            get;
        } = new CilStaticSuccessorResolver();

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

        public InstructionAttributes GetAttributes(CilInstruction instruction)
        {
            var result = InstructionAttributes.None;
            
            result |= instruction.OpCode.FlowControl switch
            {
                CilFlowControl.Branch => InstructionAttributes.CanBranch,
                CilFlowControl.ConditionalBranch => InstructionAttributes.CanBranch,
                CilFlowControl.Return => InstructionAttributes.IsTerminator,
                CilFlowControl.Throw => InstructionAttributes.IsTerminator,
                _ => InstructionAttributes.None
            };

            return result;
        }

        /// <inheritdoc />
        public int GetStackPushCount(CilInstruction instruction)
        {
            return instruction.GetStackPushCount();
        }

        /// <inheritdoc />
        public int GetStackPopCount(CilInstruction instruction)
        {
            return instruction.GetStackPopCount(_parentBody);
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetReadVariables(CilInstruction instruction)
        {
            if (instruction.IsLdloc())
            {
                return new[]
                {
                    _variables[instruction.GetLocalVariable(_parentBody.LocalVariables).Index]
                };
            }

            if (instruction.IsLdarg())
            {
                return new[]
                {
                    _parameters[instruction.GetParameter(_parentBody.Owner.Parameters).Index]
                };
            }

            return Enumerable.Empty<IVariable>();
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetWrittenVariables(CilInstruction instruction)
        {   
            if (instruction.IsStloc())
            {
                return new[]
                {
                    _variables[instruction.GetLocalVariable(_parentBody.LocalVariables).Index]
                };
            }

            if (instruction.IsStarg())
            {
                return new[]
                {
                    _parameters[instruction.GetParameter(_parentBody.Owner.Parameters).Index]
                };
            }

            return Enumerable.Empty<IVariable>();
        }
    }
}