using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class NegHandlerTest : UnaryOperatorTestBase
    {
        public NegHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(1, -1)]
        [InlineData(1000, -1000)]
        public void FlipInt32(int value, int expected) => AssertCorrect(CilOpCodes.Neg, value, expected);

        [Theory]
        [InlineData(1L, -1L)]
        [InlineData(1000L, -1000L)]
        public void FlipInt64(long value, long expected) => AssertCorrect(CilOpCodes.Neg, value, expected);
    }
}