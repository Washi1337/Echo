using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Mul"/> operation code.
    /// </summary>
    public class Mul : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Mul
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            left.F64 *= right.F64;
            context.ProgramState.Stack.Push(left);
            return DispatchResult.Success();
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction, 
            IntegerValue left, IntegerValue right)
        {
            left.Multiply(right);
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