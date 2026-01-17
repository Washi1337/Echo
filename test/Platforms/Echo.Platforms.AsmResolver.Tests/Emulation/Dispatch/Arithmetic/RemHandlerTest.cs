using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class RemHandlerTest : BinaryOperatorTestBase
    {
        public RemHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(-1000, 20, 0)]
        [InlineData(-1000, 999, -1)]
        public void RemI4ToI4(int a, int b, int expected) => AssertCorrect(CilOpCodes.Rem, a, b, expected);

        [Theory]
        [InlineData(-1000, 20L, 0L)]
        [InlineData(-1000, 999L, -1L)]
        public void RemI4ToI8(int a, long b, long expected) => AssertCorrect(CilOpCodes.Rem, a, b, expected);

        [Theory]
        [InlineData(-1000L, 20, 0L)]
        [InlineData(-1000L, 999, -1L)]
        public void RemI8ToI4(long a, int b, long expected) => AssertCorrect(CilOpCodes.Rem, a, b, expected);

        [Theory]
        [InlineData(-1000L, 20L, 0L)]
        [InlineData(-1000L, 999L, -1L)]
        public void RemI8ToI8(long a, long b, long expected) => AssertCorrect(CilOpCodes.Rem, a, b, expected);

        [Theory]
        [InlineData(1000u, 20u, 0u)]
        [InlineData(1000u, 999u, 1u)]
        public void RemU4ToU4(uint a, uint b, uint expected) => AssertCorrect(CilOpCodes.Rem_Un, a, b, expected);

        [Theory]
        [InlineData(1000u, 20ul, 0ul)]
        [InlineData(1000u, 999ul, 1ul)]
        public void RemU4ToU8(uint a, ulong b, ulong expected) => AssertCorrect(CilOpCodes.Rem_Un, a, b, expected);

        [Theory]
        [InlineData(1000ul, 20u, 0ul)]
        [InlineData(1000ul, 999u, 1ul)]
        public void RemU8ToU4(ulong a, uint b, ulong expected) => AssertCorrect(CilOpCodes.Rem_Un, a, b, expected);

        [Theory]
        [InlineData(1000ul, 20ul, 0ul)]
        [InlineData(1000ul, 999ul, 1ul)]
        public void RemU8ToU8(ulong a, ulong b, ulong expected) => AssertCorrect(CilOpCodes.Rem_Un, a, b, expected);

        [Theory]
        [InlineData(1000.0, 20.0, 0.0)]
        [InlineData(1000.0, 999.0, 1.0)]
        public void RemR8ToR8(double a, double b, double expected) => AssertCorrect(CilOpCodes.Rem, a, b, expected);

        [Fact]
        public void R4ToI4ShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(0x1234, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(0x1234, StackSlotTypeHint.Float));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Rem));
            
            Assert.False(result.IsSuccess);

            var type = result.ExceptionObject.GetObjectType();
            Assert.Equal(typeof(InvalidProgramException).FullName, type.FullName);
        }
    }
}
