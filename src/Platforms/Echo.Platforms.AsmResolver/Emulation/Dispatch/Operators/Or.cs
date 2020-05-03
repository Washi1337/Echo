using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Or"/> operation code.
    /// </summary>
    public class Or : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Or
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, Float64Value left, Float64Value right) => 
            DispatchResult.InvalidProgram();

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, IntegerValue left, IntegerValue right)
        {
            left.Or(right);
            context.ProgramState.Stack.Push(left);
            return DispatchResult.Success();
        }
    }
}