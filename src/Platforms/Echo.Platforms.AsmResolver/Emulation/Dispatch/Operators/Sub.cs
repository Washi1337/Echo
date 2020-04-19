using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Sub"/> operation code.
    /// </summary>
    public class Sub : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Sub
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, Float64Value left, Float64Value right)
        {
            left.F64 -= right.F64;
            context.ProgramState.Stack.Push(left);
            return new DispatchResult();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, IntegerValue left, IntegerValue right)
        {
            left.Subtract(right);
            context.ProgramState.Stack.Push(left);
            return new DispatchResult();
        }
    }
}