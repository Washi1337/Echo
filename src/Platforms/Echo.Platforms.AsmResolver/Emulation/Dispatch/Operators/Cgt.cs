using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Cgt"/> or <see cref="CilOpCodes.Cgt_Un"/>
    /// operation code.
    /// </summary>
    public class Cgt : ComparisonOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Cgt, CilCode.Cgt_Un
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            bool result = left.IsGreaterThan(right, instruction.OpCode.Code == CilCode.Cgt_Un);
            return ConvertToI4AndReturnSuccess(context, result);
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            IntegerValue left, IntegerValue right)
        {
            var result = left.IsGreaterThan(right, instruction.OpCode.Code == CilCode.Cgt);
            return ConvertToI4AndReturnSuccess(context, result);
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, OValue left, OValue right)
        {
            var result = left.IsGreaterThan(right);
            return ConvertToI4AndReturnSuccess(context, result);
        }
    }
}