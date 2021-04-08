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
    /// Provides a base for all integer shift operation codes.
    /// </summary>
    /// <remarks>
    /// Handlers that inherit from this class evaluate instructions with two operands and follow table III.1.6 in
    /// the ECMA-335, 6th edition (June 2012). 
    /// </remarks>
    public abstract class ShiftOperator : ICilOpCodeHandler 
    {
        /// <inheritdoc />
        public abstract IReadOnlyCollection<CilCode> SupportedOpCodes
        {
            get;
        }

        /// <inheritdoc />
        public DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var (left, right) = PopArguments(context);

            var result = (left, right) switch
            {
                (IntegerValue value, int shiftCount) => Execute(context, instruction, value, shiftCount),
                (IntegerValue value, null) => ExecuteDefault(context, value, left),
                _ => DispatchResult.InvalidProgram()
            };

            if (result.IsSuccess)
                context.ProgramState.ProgramCounter += instruction.Size;

            return result;
        }

        private static DispatchResult ExecuteDefault(CilExecutionContext context, IntegerValue value, IntegerValue left)
        {
            if (value.IsNonZero.IsUnknown)
                value.MarkFullyUnknown();

            context.ProgramState.Stack.Push((ICliValue) left);
            return DispatchResult.Success();
        }

        /// <summary>
        /// Performs the operation on the two pushed integers.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="value">The pushed integer value to shift</param>
        /// <param name="shiftCount">The amount to shift the value with.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(CilExecutionContext context, CilInstruction instruction,
            IntegerValue value, int shiftCount);
        
        private static (IntegerValue, int?) PopArguments(CilExecutionContext context)
        {
            var value2 = context.ProgramState.Stack.Pop();
            var value1 = context.ProgramState.Stack.Pop();

            int? shiftCount = null;
            if (value2.IsKnown)
            {
                shiftCount = value2 switch
                {
                    I4Value int32 => int32.I32,
                    NativeIntegerValue nativeInt => (int?) nativeInt.ToKnownI64(),
                    _ => null
                };
            }

            return (value1 as IntegerValue, shiftCount);
        }
    }
}