using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    public abstract class ComparisonOperator : BinaryNumericOperator
    {
        protected static DispatchResult ConvertToI4AndReturnSuccess(ExecutionContext context, Trilean result)
        {
            var i4Result = new I4Value(result.ToBooleanOrFalse() ? 1 : 0, 0xFFFFFFFEu | (result.IsKnown ? 1u : 0u));
            context.ProgramState.Stack.Push(i4Result);
            return DispatchResult.Success();
        }
    }
}