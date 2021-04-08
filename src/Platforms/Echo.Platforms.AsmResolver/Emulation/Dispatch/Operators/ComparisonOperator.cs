using Echo.Concrete.Emulation;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a base for all comparison operation codes.
    /// </summary>
    public abstract class ComparisonOperator : BinaryNumericOperator
    {
        /// <summary>
        /// Converts the provided trilean to an I4 stack value, pushes it onto the stack and returns the success
        /// dispatcher result.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="result">The trilean value.</param>
        /// <returns>The dispatch result.</returns>
        protected static DispatchResult ConvertToI4AndReturnSuccess(CilExecutionContext context, Trilean result)
        {
            var i4Result = new I4Value(result.ToBooleanOrFalse() ? 1 : 0, 0xFFFFFFFEu | (result.IsKnown ? 1u : 0u));
            context.ProgramState.Stack.Push(i4Result);
            return DispatchResult.Success();
        }
    }
}