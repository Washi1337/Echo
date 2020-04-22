using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class MulTest : DispatcherTestBase
    {
        public MulTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void MulInt32ToInt32ShouldResultInInt32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(5678));
            stack.Push(new Integer32Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Integer32Value>(stack.Top);
            Assert.Equal(7006652, ((Integer32Value) stack.Top).I32);
        }

        [Fact]
        public void MulInt64ToInt64ShouldResultInInt64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer64Value(5678));
            stack.Push(new Integer64Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Integer64Value>(stack.Top);
            Assert.Equal(7006652, ((Integer64Value) stack.Top).I64);
        }

        [Fact]
        public void MulInt32ToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(5678, is32Bit));
            stack.Push(new Integer32Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(7006652, is32Bit), stack.Top);
        }

        [Fact]
        public void MulNativeIntToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(5678, is32Bit));
            stack.Push(new NativeIntegerValue(1234, is32Bit));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(7006652, is32Bit), stack.Top);
        }

        [Fact]
        public void MulFloatToFloatShouldResultInFloat()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Float64Value(5.678));
            stack.Push(new Float64Value(1.234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Float64Value>(stack.Top);
            Assert.Equal(new Float64Value(7.006652D), stack.Top);
        }

        [Fact]
        public void MulInt32ToInt64ShouldThrow()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(5678));
            stack.Push(new Integer64Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.False(result.IsSuccess);
        }
    }
}