using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a base for all unary numeric operation codes.
    /// </summary>
    /// <remarks>
    /// Handlers that inherit from this class evaluate instructions with two operands and follow table III.1.3 in
    /// the ECMA-335, 6th edition (June 2012). 
    /// </remarks>
    public abstract class UnaryNumericOperator : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public abstract IReadOnlyCollection<CilCode> SupportedOpCodes
        {
            get;
        }

        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var argument = context.ProgramState.Stack.Pop();

            var result = argument switch
            {
                Float64Value float64Value => Execute(context, float64Value),
                IntegerValue integerValue => Execute(context, integerValue),
                _ => throw new InvalidProgramException()
            };

            if (result.IsSuccess)
                context.ProgramState.ProgramCounter += instruction.Size;

            return result;
        }
        
        /// <summary>
        /// Performs the operation on the pushed floating point value.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="value">The pushed value to perform the operation on.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(ExecutionContext context, Float64Value value);

        /// <summary>
        /// Performs the operation on the pushed integer value.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="value">The pushed value to perform the operation on.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(ExecutionContext context, IntegerValue value);
    }
}