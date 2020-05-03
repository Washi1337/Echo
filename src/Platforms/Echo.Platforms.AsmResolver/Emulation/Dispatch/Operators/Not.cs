using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Not"/> operation code.
    /// </summary>
    public class Not : UnaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Not
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, Float64Value value) =>
            DispatchResult.InvalidProgram();

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, IntegerValue value)
        {
            value.Not();
            context.ProgramState.Stack.Push(value);
            return DispatchResult.Success();
        }
    }
}