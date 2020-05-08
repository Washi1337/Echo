using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ble"/>, <see cref="CilOpCodes.Ble_S"/>,
    /// <see cref="CilOpCodes.Ble_Un"/>, or <see cref="CilOpCodes.Ble_Un_S"/> operation code.
    /// </summary>
    public class Ble : BinaryBranchHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ble, CilCode.Ble_S, CilCode.Ble_Un, CilCode.Ble_Un_S
        };

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, CilInstruction instruction,
            IntegerValue left, IntegerValue right)
        {
            bool? equal = left.IsEqualTo(right);
            bool? lessThan = left.IsLessThan(right, IsSigned(instruction));

            if (equal.GetValueOrDefault() || lessThan.GetValueOrDefault())
                return true;
            if (!equal.HasValue || !lessThan.HasValue)
                return null;
            return false;
        }

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool equal = left.F64 == right.F64;
            bool lessThan = left.IsLessThan(right, IsSigned(instruction));

            return lessThan || equal;
        }

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, CilInstruction instruction, 
            OValue left, OValue right)
        {
            bool? equal = left.IsEqualTo(right);
            bool? lessThan = left.IsLessThan(right);

            if (equal.GetValueOrDefault() || lessThan.GetValueOrDefault())
                return true;
            if (!equal.HasValue || !lessThan.HasValue)
                return null;
            return false;
        }

        private static bool IsSigned(CilInstruction instruction)
        {
            var code = instruction.OpCode.Code;
            return code == CilCode.Ble || code == CilCode.Ble_S;
        }
    }
}