using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Core;
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
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction,
            IntegerValue left, IntegerValue right)
        {
            var equal = left.IsEqualTo(right);
            var greaterThan = left.IsGreaterThan(right, IsSigned(instruction));

            if (equal.ToBooleanOrFalse() || greaterThan.ToBooleanOrFalse())
                return Trilean.True;
            if (!equal.IsKnown || !greaterThan.IsKnown)
                return Trilean.Unknown;
            return Trilean.False;
        }

        /// <inheritdoc />
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool equal = left.F64 == right.F64;
            bool greaterThan = left.IsGreaterThan(right, IsSigned(instruction));

            return greaterThan || equal;
        }

        /// <inheritdoc />
        protected override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction, 
            OValue left, OValue right)
        {
            var equal = left.IsEqualTo(right);
            var greaterThan = left.IsGreaterThan(right);

            if (equal.ToBooleanOrFalse() || greaterThan.ToBooleanOrFalse())
                return Trilean.True;
            if (!equal.IsKnown || !greaterThan.IsKnown)
                return Trilean.Unknown;
            return Trilean.False;
        }

        private static bool IsSigned(CilInstruction instruction)
        {
            var code = instruction.OpCode.Code;
            return code == CilCode.Bge || code == CilCode.Bge_S;
        }
    }
}