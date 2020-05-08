using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ceq"/> operation code.
    /// </summary>
    public class Ceq : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ceq
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            var i4Result = new I4Value(left.F64 == right.F64 ? 1 : 0);
            context.ProgramState.Stack.Push(i4Result);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            IntegerValue left, IntegerValue right)
        {
            var result = left.IsEqualTo(right);
            
            var i4Result = new I4Value(result.GetValueOrDefault() ? 1 : 0, 0xFFFFFFFEu | (result.HasValue ? 1u : 0u));
            context.ProgramState.Stack.Push(i4Result);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            OValue left, OValue right)
        {
            bool? result;

            if (left.IsKnown && right.IsKnown)
                result = ReferenceEquals(left.ObjectValue, right.ObjectValue);
            else
                result = null;

            var i4Result = new I4Value(result.GetValueOrDefault() ? 1 : 0, 0xFFFFFFFEu | (result.HasValue ? 1u : 0u));
            context.ProgramState.Stack.Push(i4Result);
            return DispatchResult.Success();
        }
    }
}