using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class DivHandlerTest : BinaryOperatorTestBase
    {
        public DivHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void DivI4ToI4() => AssertCorrect(CilOpCodes.Div, 1000u, 20u, 50u);

        [Theory]
        [InlineData(1000u, 20ul, 50ul)]
        public void DivI4ToI8(uint a, ulong b, ulong expected) => AssertCorrect(CilOpCodes.Div, a, b, expected);

        [Theory]
        [InlineData(1000ul, 20u, 50ul)]
        public void DivI8ToI4(ulong a, uint b, ulong expected) => AssertCorrect(CilOpCodes.Div, a, b, expected);

        [Fact]
        public void DivI8ToI8() => AssertCorrect(CilOpCodes.Div, 1000ul, 20ul, 50ul);

        [Fact]
        public void DivR8ToR8() => AssertCorrect(CilOpCodes.Div, 1000.0, 20.0, 50.0);

        [Fact]
        public void DivR4ToI4ShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(0x1234, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(0x1234, StackSlotTypeHint.Float));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Div));
            
            Assert.False(result.IsSuccess);

            var type = result.ExceptionPointer!.AsSpan().GetObjectPointerType(Context.Machine);
            Assert.Equal(typeof(InvalidProgramException).FullName, type.FullName);
        }
    }
}