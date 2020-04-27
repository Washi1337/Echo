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
            stack.Push(new I4Value(5678));
            stack.Push(new I4Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<I4Value>(stack.Top);
            Assert.Equal(6912, ((I4Value) stack.Top).I32);
        }

        [Fact]
        public void AddInt64ToInt64ShouldResultInInt64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(5678));
            stack.Push(new I8Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<I8Value>(stack.Top);
            Assert.Equal(6912, ((I8Value) stack.Top).I64);
        }

        [Fact]
        public void AddInt32ToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(5678, is32Bit));
            stack.Push(new I4Value(1234));
            
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
            stack.Push(new FValue(5.678));
            stack.Push(new FValue(1.234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<FValue>(stack.Top);
            Assert.Equal(new FValue(6.912), stack.Top);
        }

        [Fact]
        public void AddInt32ToInt64ShouldThrow()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(5678));
            stack.Push(new I8Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Add));
            
            Assert.False(result.IsSuccess);
        }
    }
}