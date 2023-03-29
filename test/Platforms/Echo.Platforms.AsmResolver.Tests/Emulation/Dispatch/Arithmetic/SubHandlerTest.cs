using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class SubHandlerTest : BinaryOperatorTestBase
    {
        public SubHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void SubI4ToI4() => AssertCorrect(CilOpCodes.Sub, 0x1234, 0x234u, 0x1000u);

        [Theory]
        [InlineData(1234, 234, 1000)]
        [InlineData(0x0000_0000, 1, 0xffff_ffff_ffff_ffff)]
        public void SubI4ToI8(uint a, ulong b, ulong expected) => AssertCorrect(CilOpCodes.Sub, a, b, expected);

        [Theory]
        [InlineData(1234, 234, 1000)]
        [InlineData(0x0000_0000, 1, 0xffff_ffff_ffff_ffff)]
        public void SubI8ToI4(ulong a, uint b, ulong expected) => AssertCorrect(CilOpCodes.Sub, a, b, expected);

        [Fact]
        public void SubI8ToI8() => AssertCorrect(CilOpCodes.Sub, 0x1234ul, 0x1000ul, 0x234ul);

        [Fact]
        public void SubR8ToR8() => AssertCorrect(CilOpCodes.Sub, 6.912D, 1.234D, 5.678D);

        [Fact]
        public void SubR4ToI4ShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(0x1234, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(0x1234, StackSlotTypeHint.Float));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Sub));
            
            Assert.False(result.IsSuccess);

            var type = result.ExceptionPointer!.ToObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal(typeof(InvalidProgramException).FullName, type.FullName);
        }
    }
}