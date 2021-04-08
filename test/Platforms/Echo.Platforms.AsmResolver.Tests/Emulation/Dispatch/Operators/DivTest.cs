using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class DivTest : DispatcherTestBase
    {
        public DivTest(MockModuleFixture moduleFixture)
            : base(moduleFixture) { }

        [Fact]
        public void DivInt32ToInt32ShouldResultInInt32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(69));
            stack.Push(new I4Value(3));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Div));

            Assert.True(result.IsSuccess);
            var i4Value = Assert.IsAssignableFrom<I4Value>(stack.Top);
            Assert.Equal(23, i4Value.I32);
        }

        [Fact]
        public void DividingByZeroShouldThrowException()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(123));
            stack.Push(new I4Value(0));

            Assert.Throws<DivideByZeroException>(() => Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Div)));
        }

        [Fact]
        public void DivInt64ToInt64ShouldResultInInt64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(69));
            stack.Push(new I8Value(3));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Div));

            Assert.True(result.IsSuccess);
            var i8Value = Assert.IsAssignableFrom<I8Value>(stack.Top);
            Assert.Equal(23, i8Value.I64);
        }

        [Fact]
        public void DivInt32ToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;

            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(69, is32Bit));
            stack.Push(new I4Value(3));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Div));

            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(23, is32Bit), stack.Top);
        }

        [Fact]
        public void DivNativeIntToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;

            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(69, is32Bit));
            stack.Push(new NativeIntegerValue(3, is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Div));

            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(23, is32Bit), stack.Top);
        }

        [Fact]
        public void DivFloatToFloatShouldResultInFloat()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new FValue(69.0));
            stack.Push(new FValue(3.0));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Div));

            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<FValue>(stack.Top);
            Assert.Equal(new FValue(23.0), stack.Top);
        }

        [Fact]
        public void DivInt32ToInt64ShouldThrow()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(69));
            stack.Push(new I8Value(420));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Div));

            Assert.False(result.IsSuccess);
        }
    }
}