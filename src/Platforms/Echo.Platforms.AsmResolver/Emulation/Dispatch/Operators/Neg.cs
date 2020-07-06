using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Neg"/> operation code.
    /// </summary>
    public class Neg : UnaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Neg
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, Float64Value value)
        {
            value.F64 = -value.F64;
            context.ProgramState.Stack.Push(value);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, IntegerValue value)
        {
            value.TwosComplement();
            context.ProgramState.Stack.Push(value);
            return DispatchResult.Success();
        }
    }
}