using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Div"/> operation code.
    /// </summary>
    public class Div : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Div
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(
            ExecutionContext context, CilInstruction instruction, FValue left, FValue right)
        {
            left.F64 /= right.F64;
            context.ProgramState.Stack.Push(left);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(
            ExecutionContext context, CilInstruction instruction, IntegerValue left, IntegerValue right)
        {
            left.Divide(right);
            context.ProgramState.Stack.Push(left);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(
            ExecutionContext context, CilInstruction instruction, OValue left, OValue right)
        {
            return DispatchResult.InvalidProgram();
        }
    }
}