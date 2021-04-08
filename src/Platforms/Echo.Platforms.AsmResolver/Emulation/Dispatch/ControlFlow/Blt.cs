using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Blt"/>, <see cref="CilOpCodes.Blt_S"/>,
    /// <see cref="CilOpCodes.Blt_Un"/>, or <see cref="CilOpCodes.Blt_Un_S"/> operation code.
    /// </summary>
    public class Blt : BinaryBranchHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Blt, CilCode.Blt_S, CilCode.Blt_Un, CilCode.Blt_Un_S
        };

        /// <inheritdoc />
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction,
            IntegerValue left, IntegerValue right)
        {
            return left.IsLessThan(right, IsSigned(instruction));
        }

        /// <inheritdoc />
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction, FValue left,
            FValue right)
        {
            return left.IsLessThan(right, IsSigned(instruction));
        }

        /// <inheritdoc />
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction, OValue left,
            OValue right)
        {
            return left.IsLessThan(right);
        }

        private static bool IsSigned(CilInstruction instruction)
        {
            var code = instruction.OpCode.Code;
            return code == CilCode.Blt || code == CilCode.Blt_S;
        }
        
    }
}