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
            stack.Push(new I4Value(5678));
            stack.Push(new I4Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<I4Value>(stack.Top);
            Assert.Equal(7006652, ((I4Value) stack.Top).I32);
        }

        [Fact]
        public void MulInt64ToInt64ShouldResultInInt64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(5678));
            stack.Push(new I8Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<I8Value>(stack.Top);
            Assert.Equal(7006652, ((I8Value) stack.Top).I64);
        }

        [Fact]
        public void MulInt32ToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(5678, is32Bit));
            stack.Push(new I4Value(1234));
            
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
            stack.Push(new FValue(5.678));
            stack.Push(new FValue(1.234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<FValue>(stack.Top);
            Assert.Equal(new FValue(7.006652D), stack.Top);
        }

        [Fact]
        public void MulInt32ToInt64ShouldThrow()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(5678));
            stack.Push(new I8Value(1234));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Mul));
            
            Assert.False(result.IsSuccess);
        }
    }
}