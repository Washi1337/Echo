using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a base for all binary numeric operation codes.
    /// </summary>
    /// <remarks>
    /// Handlers that inherit from this class evaluate instructions with two operands and follow table III.1.2 in
    /// the ECMA-335, 6th edition (June 2012). 
    /// </remarks>
    public abstract class BinaryNumericOperator : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public abstract IReadOnlyCollection<CilCode> SupportedOpCodes
        {
            get;
        }

        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var (left, right) = PopArguments(context);

            var result = (left, right) switch
            {
                (IntegerValue a, IntegerValue b) => Execute(context, a, b),
                (Float64Value a, Float64Value b) => Execute(context, a, b),
                _ => throw new InvalidProgramException()
            };

            if (result.IsSuccess)
                context.ProgramState.ProgramCounter += instruction.Size;

            return result;
        }

        /// <summary>
        /// Performs the operation on the two pushed floating point values.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="left">The left side of the operation.</param>
        /// <param name="right">The right side of the operation.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(ExecutionContext context, Float64Value left, Float64Value right);

        /// <summary>
        /// Performs the operation on the two pushed integers.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="left">The left side of the operation.</param>
        /// <param name="right">The right side of the operation.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(ExecutionContext context, IntegerValue left, IntegerValue right);

        private static (IConcreteValue, IConcreteValue) PopArguments(ExecutionContext context)
        {
            var value2 = context.ProgramState.Stack.Pop();
            var value1 = context.ProgramState.Stack.Pop();

            return (value1, value2) switch
            {
                (Integer32Value a, Integer32Value b) => (a, b),
                (Integer64Value a, Integer64Value b) => (a, b),
                (NativeIntegerValue a, NativeIntegerValue b) => (a, b),
                (Float64Value a, Float64Value b) => (a, b),

                (Integer32Value a, NativeIntegerValue b) => (new NativeIntegerValue(a, b.Size == 4), b),
                (NativeIntegerValue a, Integer32Value b) => (a, new NativeIntegerValue(b, a.Size == 4)),

                _ => (null, null),
            };
        }
        
    }
}