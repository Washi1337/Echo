using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    internal static class BinaryOperationHelper
    {
        public static (ICliValue left, ICliValue right) PopBinaryOperationArguments(ExecutionContext context)
        {
            var values = PeekBinaryOperationArguments(context);
            var stack = context.ProgramState.Stack;
            stack.Pop();
            stack.Pop();
            return values;
        }
        
        public static (ICliValue left, ICliValue right) PeekBinaryOperationArguments(ExecutionContext context)
        {
            var stack = context.ProgramState.Stack;
            var value2 = stack[0];
            var value1 = stack[1];

            return (value1, value2) switch
            {
                (I4Value a, I4Value b) => (a, b),
                (I8Value a, I8Value b) => (a, b),
                (NativeIntegerValue a, NativeIntegerValue b) => (a, b),
                (FValue a, FValue b) => (a, b),
                (OValue a, OValue b) => (a, b),
                
                (I4Value a, NativeIntegerValue b) => (new NativeIntegerValue(a, b.Size == 4), b),
                (NativeIntegerValue a, I4Value b) => (a, new NativeIntegerValue(b, a.Size == 4)),
                
                _ => (null, null),
            };
        }
        
    }
}