using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class SubTest : DispatcherTestBase
    {
        public SubTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void SubInt32ToInt32ShouldResultInInt32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(5678));
            stack.Push(new Integer32Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Sub));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Integer32Value>(stack.Top);
            Assert.Equal(4444, ((Integer32Value) stack.Top).I32);
        }

        [Fact]
        public void SubInt64ToInt64ShouldResultInInt64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer64Value(5678));
            stack.Push(new Integer64Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Sub));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Integer64Value>(stack.Top);
            Assert.Equal(4444, ((Integer64Value) stack.Top).I64);
        }

        [Fact]
        public void SubInt32ToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(5678, is32Bit));
            stack.Push(new Integer32Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Sub));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(4444, is32Bit), stack.Top);
        }

        [Fact]
        public void SubNativeIntToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(5678, is32Bit));
            stack.Push(new NativeIntegerValue(1234, is32Bit));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Sub));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(4444, is32Bit), stack.Top);
        }

        [Fact]
        public void SubFloatToFloatShouldResultInFloat()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Float64Value(5.678));
            stack.Push(new Float64Value(1.234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Sub));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<Float64Value>(stack.Top);
            Assert.Equal(new Float64Value(4.444), stack.Top);
        }

        [Fact]
        public void SubInt32ToInt64ShouldThrow()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(5678));
            stack.Push(new Integer64Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Sub));
            
            Assert.False(result.IsSuccess);
        }
    }
}