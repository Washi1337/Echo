using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.And"/> operation code.
    /// </summary>
    public class And : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.And
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, Float64Value left, Float64Value right) => 
            new DispatchResult(new InvalidProgramException());

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, IntegerValue left, IntegerValue right)
        {
            left.And(right);
            context.ProgramState.Stack.Push(left);
            return new DispatchResult();
        }
    }
}