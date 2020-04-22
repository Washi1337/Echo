using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class AddTest : DispatcherTestBase
    {
        public AddTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void AddInt32ToInt32ShouldResultInInt32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(5678));
            stack.Push(new Integer32Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Integer32Value>(stack.Top);
            Assert.Equal(6912, ((Integer32Value) stack.Top).I32);
        }

        [Fact]
        public void AddInt64ToInt64ShouldResultInInt64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer64Value(5678));
            stack.Push(new Integer64Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Integer64Value>(stack.Top);
            Assert.Equal(6912, ((Integer64Value) stack.Top).I64);
        }

        [Fact]
        public void AddInt32ToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(5678, is32Bit));
            stack.Push(new Integer32Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(6912, is32Bit), stack.Top);
        }

        [Fact]
        public void AddNativeIntToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(5678, is32Bit));
            stack.Push(new NativeIntegerValue(1234, is32Bit));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(6912, is32Bit), stack.Top);
        }

        [Fact]
        public void AddFloatToFloatShouldResultInFloat()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Float64Value(5.678));
            stack.Push(new Float64Value(1.234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Float64Value>(stack.Top);
            Assert.Equal(new Float64Value(6.912), stack.Top);
        }

        [Fact]
        public void AddInt32ToInt64ShouldThrow()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(5678));
            stack.Push(new Integer64Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.False(result.IsSuccess);
        }
    }
}