using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class RemTest : DispatcherTestBase
    {
        public RemTest(MockModuleFixture moduleFixture)
            : base(moduleFixture) { }

        [Fact]
        public void RemInt32ToInt32ShouldResultInInt32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(420));
            stack.Push(new I4Value(69));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Rem));

            Assert.True(result.IsSuccess);
            var i4Value = Assert.IsAssignableFrom<I4Value>(stack.Top);
            Assert.Equal(6, i4Value.I32);
        }

        [Fact]
        public void RemainderWithZeroDivisorShouldThrowException()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(420));
            stack.Push(new I4Value(0));

            Assert.Throws<DivideByZeroException>(() =>
                Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Rem)));
        }

        [Fact]
        public void RemInt64ToInt64ShouldResultInInt64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(420));
            stack.Push(new I8Value(69));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Rem));

            Assert.True(result.IsSuccess);
            var i8Value = Assert.IsAssignableFrom<I8Value>(stack.Top);
            Assert.Equal(6, i8Value.I64);
        }

        [Fact]
        public void RemInt32ToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;

            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(420, is32Bit));
            stack.Push(new I4Value(69));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Rem));

            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(6, is32Bit), stack.Top);
        }

        [Fact]
        public void DivNativeIntToNativeIntShouldResultInNativeInt()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;

            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(420, is32Bit));
            stack.Push(new NativeIntegerValue(69, is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Rem));

            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<NativeIntegerValue>(stack.Top);
            Assert.Equal(new NativeIntegerValue(6, is32Bit), stack.Top);
        }

        [Fact]
        public void RemFloatToFloatShouldResultInFloat()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new FValue(420.0));
            stack.Push(new FValue(69.0));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Rem));

            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<FValue>(stack.Top);
            Assert.Equal(new FValue(6.0), stack.Top);
        }

        [Fact]
        public void RemInt32ToInt64ShouldThrow()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(420));
            stack.Push(new I8Value(69));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Div));

            Assert.False(result.IsSuccess);
        }
    }
}