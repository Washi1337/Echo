using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class AddHandlerTest : BinaryOperatorTestBase
    {
        public AddHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void AddI4ToI4() => AssertCorrect(CilOpCodes.Add, 0x1000u, 0x234u, 0x1234);

        [Theory]
        [InlineData(1000, 234, 1234)]
        [InlineData(0x8000_0000, 1, 0xffff_ffff_8000_0001)]
        public void AddI4ToI8(uint a, ulong b, ulong expected) => AssertCorrect(CilOpCodes.Add, a, b, expected);

        [Theory]
        [InlineData(234, 1000, 1234)]
        [InlineData(1, 0x8000_0000, 0xffff_ffff_8000_0001)]
        public void AddI8ToI4(ulong a, uint b, ulong expected) => AssertCorrect(CilOpCodes.Add, a, b, expected);

        [Fact]
        public void AddI8ToI8() => AssertCorrect(CilOpCodes.Add, 0x1000ul, 0x234ul, 0x1234);

        [Fact]
        public void AddR8ToR8() => AssertCorrect(CilOpCodes.Add, 1.234D, 5.678D, 6.912D);

        [Fact]
        public void AddR4ToI4ShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(0x1234, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(0x1234, StackSlotTypeHint.Float));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Add));
            
            Assert.False(result.IsSuccess);

            var type = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal(typeof(InvalidProgramException).FullName, type.FullName);
        }
    }
}