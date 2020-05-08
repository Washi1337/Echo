using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Cgt"/> or <see cref="CilOpCodes.Cgt_Un"/>
    /// operation code.
    /// </summary>
    public class Cgt : BinaryNumericOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Cgt, CilCode.Cgt_Un
        };

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            FValue left, FValue right)
        {
            bool result = left.IsGreaterThan(right, instruction.OpCode.Code == CilCode.Cgt_Un);
            return ConvertToI4AndReturnSuccess(context, result);
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, 
            IntegerValue left, IntegerValue right)
        {
            bool? result = left.IsGreaterThan(right, instruction.OpCode.Code == CilCode.Cgt);
            return ConvertToI4AndReturnSuccess(context, result);
        }

        /// <inheritdoc />
        protected override DispatchResult Execute(ExecutionContext context, CilInstruction instruction, OValue left, OValue right)
        {
            // cgt[.un] can be used on object references for null checks. 
            
            bool? result = left switch
            {
                {IsZero: false} when right is { IsZero: true } => true,
                {IsZero: true} when right is { IsZero: false } => false,
                _ => null
            };

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