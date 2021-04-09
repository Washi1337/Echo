using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

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
        protected override DispatchResult Execute(CilExecutionContext context, FValue value) =>
            DispatchResult.InvalidProgram();

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, IntegerValue value)
        {
            value.Not();
            context.ProgramState.Stack.Push((ICliValue) value);
            return DispatchResult.Success();
        }
    }
}