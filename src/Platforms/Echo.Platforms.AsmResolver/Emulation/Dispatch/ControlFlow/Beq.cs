using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Beq"/> and <see cref="CilOpCodes.Beq_S"/>
    /// operation codes.
    /// </summary>
    public class Beq : BinaryBranchHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Beq, CilCode.Beq_S
        };

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, IntegerValue left, IntegerValue right)
        {
            return left.IsEqualTo(right);
        }

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, FValue left, FValue right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return left.F64 == right.F64;
        }

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, OValue left, OValue right)
        {
            return left.IsKnown && right.IsKnown
                ? (bool?) ReferenceEquals(left.ObjectValue, right.ObjectValue)
                : null;
        }
    }
}