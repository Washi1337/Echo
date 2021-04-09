using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

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
        protected override DispatchResult Execute(CilExecutionContext context, FValue value)
        {
            value.F64 = -value.F64;
            context.ProgramState.Stack.Push(value);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, IntegerValue value)
        {
            value.TwosComplement();
            context.ProgramState.Stack.Push((ICliValue) value);
            return DispatchResult.Success();
        }
    }
}