using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Clt"/> or <see cref="CilOpCodes.Clt_Un"/>
    /// operation code.
    /// </summary>
    public class Clt : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Clt, CilCode.Clt_Un
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            bool result = left.IsLessThan(right, instruction.OpCode.Code == CilCode.Clt_Un);
            return ConvertToI4AndReturnSuccess(context, result);
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            IntegerValue left, IntegerValue right)
        {
            bool? result = left.IsLessThan(right, instruction.OpCode.Code == CilCode.Clt);
            return ConvertToI4AndReturnSuccess(context, result);
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, OValue left, OValue right)
        {
            bool? result = left.IsLessThan(right);
            return ConvertToI4AndReturnSuccess(context, result);
        }

        private static DispatchResult ConvertToI4AndReturnSuccess(ExecutionContext context, bool? result)
        {
            var i4Result = new I4Value(result.GetValueOrDefault() ? 1 : 0, 0xFFFFFFFEu | (result.HasValue ? 1u : 0u));
            context.ProgramState.Stack.Push(i4Result);
            return DispatchResult.Success();
        }
        
    }
}