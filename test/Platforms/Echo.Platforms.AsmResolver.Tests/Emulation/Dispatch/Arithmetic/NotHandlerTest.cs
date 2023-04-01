using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class NotHandlerTest : UnaryOperatorTestBase
    {
        public NotHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(0, unchecked( (int)0xFFFFFFFF))]
        [InlineData(unchecked( (int)0xFFFFFFFF), 0)]
        [InlineData(unchecked( (int)0xFFFF0000), 0x0000FFFF)]
        public void FlipInt32(int value, int expected) => AssertCorrect(CilOpCodes.Not, value, expected);

        [Theory]
        [InlineData(0, unchecked((long) 0xFFFFFFFFFFFFFFFF))]
        [InlineData(unchecked((long) 0xFFFFFFFFFFFFFFFF), 0)]
        [InlineData(unchecked((long) 0xFFFF0000FFFF0000), 0x0000FFFF0000FFFF)]
        public void FlipInt64(long value, long expected) => AssertCorrect(CilOpCodes.Not, value, expected);
    }
}