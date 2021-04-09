using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Bgt"/>, <see cref="CilOpCodes.Bgt_S"/>,
    /// <see cref="CilOpCodes.Bgt_Un"/>, or <see cref="CilOpCodes.Bgt_Un_S"/> operation code.
    /// </summary>
    public class Bgt : BinaryBranchHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Bgt, CilCode.Bgt_S, CilCode.Bgt_Un, CilCode.Bgt_Un_S
        };

        /// <inheritdoc />
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction,
            IntegerValue left, IntegerValue right)
        {
            return left.IsGreaterThan(right, IsSigned(instruction));
        }

        /// <inheritdoc />
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction, FValue left,
            FValue right)
        {
            return left.IsGreaterThan(right, IsSigned(instruction));
        }

        /// <inheritdoc />
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction, OValue left,
            OValue right)
        {
            return left.IsGreaterThan(right);
        }

        private static bool IsSigned(CilInstruction instruction)
        {
            var code = instruction.OpCode.Code;
            return code == CilCode.Bgt || code == CilCode.Bgt_S;
        }
        
    }
}