using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

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
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction,
            FValue left, FValue right)
        {
            return DispatchResult.InvalidProgram();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction,
            IntegerValue left, IntegerValue right)
        {
            left.And(right);
            context.ProgramState.Stack.Push((ICliValue) left);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction,
            OValue left, OValue right)
        {
            return DispatchResult.InvalidProgram();
        }
    }
}