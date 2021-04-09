using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ceq"/> operation code.
    /// </summary>
    public class Ceq : ComparisonOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ceq
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            var i4Result = new I4Value(left.F64 == right.F64 ? 1 : 0);
            context.ProgramState.Stack.Push(i4Result);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction, 
            IntegerValue left, IntegerValue right)
        {
            var result = left.IsEqualTo(right);
            return ConvertToI4AndReturnSuccess(context, result);
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction, 
            OValue left, OValue right)
        {
            var result = left.IsEqualTo(right);
            return ConvertToI4AndReturnSuccess(context, result);
        }
    }
}