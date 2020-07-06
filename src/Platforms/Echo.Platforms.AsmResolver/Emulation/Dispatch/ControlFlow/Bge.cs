using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Bge"/>, <see cref="CilOpCodes.Bge_S"/>,
    /// <see cref="CilOpCodes.Bge_Un"/>, or <see cref="CilOpCodes.Bge_Un_S"/> operation code.
    /// </summary>
    public class Bge : BinaryBranchHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Bge, CilCode.Bge_S, CilCode.Bge_Un, CilCode.Bge_Un_S
        };

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, CilInstruction instruction,
            IntegerValue left, IntegerValue right)
        {
            bool? equal = left.IsEqualTo(right);
            bool? greaterThan = left.IsGreaterThan(right, IsSigned(instruction));

            if (equal.GetValueOrDefault() || greaterThan.GetValueOrDefault())
                return true;
            if (!equal.HasValue || !greaterThan.HasValue)
                return null;
            return false;
        }

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool equal = left.F64 == right.F64;
            bool greaterThan = left.IsGreaterThan(right, IsSigned(instruction));

            return greaterThan || equal;
        }

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, CilInstruction instruction, 
            OValue left, OValue right)
        {
            bool? equal = left.IsEqualTo(right);
            bool? greaterThan = left.IsGreaterThan(right);

            if (equal.GetValueOrDefault() || greaterThan.GetValueOrDefault())
                return true;
            if (!equal.HasValue || !greaterThan.HasValue)
                return null;
            return false;
        }

        private static bool IsSigned(CilInstruction instruction)
        {
            var code = instruction.OpCode.Code;
            return code == CilCode.Bge || code == CilCode.Bge_S;
        }
    }
}