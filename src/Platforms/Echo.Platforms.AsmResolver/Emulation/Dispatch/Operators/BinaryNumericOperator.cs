using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

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
            var (left, right) = BinaryOperationHelper.PopBinaryOperationArguments(context);

            var result = (left, right) switch
            {
                (IntegerValue a, IntegerValue b) => Execute(context, instruction, a, b),
                (FValue a, FValue b) => Execute(context, instruction, a, b),
                (OValue a, OValue b) => Execute(context, instruction, a, b),
                _ => DispatchResult.InvalidProgram(),
            };

            if (result.IsSuccess)
                context.ProgramState.ProgramCounter += instruction.Size;

            return result;
        }

        /// <summary>
        /// Performs the operation on the two pushed floating point values.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="left">The left side of the operation.</param>
        /// <param name="right">The right side of the operation.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(ExecutionContext context, CilInstruction instruction, FValue left, FValue right);

        /// <summary>
        /// Performs the operation on the two pushed integers.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="left">The left side of the operation.</param>
        /// <param name="right">The right side of the operation.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(ExecutionContext context, CilInstruction instruction, IntegerValue left, IntegerValue right);

        /// <summary>
        /// Performs the operation on the two pushed object references.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="left">The left side of the operation.</param>
        /// <param name="right">The right side of the operation.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(ExecutionContext context, CilInstruction instruction, OValue left, OValue right);
    }
}