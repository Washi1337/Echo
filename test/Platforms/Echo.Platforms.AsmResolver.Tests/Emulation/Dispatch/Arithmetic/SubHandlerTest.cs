using System;
using AsmResolver.PE.DotNet.Cil;
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
        public void SubI4ToI4() => AssertCorrect(CilOpCodes.Sub, 0x1234, 0x234, 0x1000);

        [Fact]
        public void SubI4ToI8() => AssertCorrect(CilOpCodes.Sub, 1234, 234L, 1000L);

        [Fact]
        public void SubU4ToU8() => AssertCorrect(CilOpCodes.Sub, 0x0000_0000, 1L, 0xffff_ffff_ffff_ffffL);
        
        [Fact]
        public void SubI8ToI4() => AssertCorrect(CilOpCodes.Sub, 1234L, 234, 1000L);
        
        [Fact]
        public void SubU8ToU4() => AssertCorrect(CilOpCodes.Sub, 0x0000_0000L, 1, 0xffff_ffff_ffff_ffffL);
        
        [Fact]
        public void SubI8ToI8() => AssertCorrect(CilOpCodes.Sub, 0x1234L, 0x1000L, 0x234L);

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

            var type = result.ExceptionObject.GetObjectType();
            Assert.Equal(typeof(InvalidProgramException).FullName, type.FullName);
        }
    }
}