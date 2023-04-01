using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class ShlHandlerTest : BinaryOperatorTestBase
    {
        public ShlHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(1, 0, 1)]
        [InlineData(1, 4, 1 << 4)]
        public void ShiftInt32(uint a, uint b, uint expected) => AssertCorrect(CilOpCodes.Shl, a, b, expected);

        [Theory]
        [InlineData(1L, 0, 1L)]
        [InlineData(1L, 4, 1L << 4)]
        public void ShiftInt64(ulong a, ulong b, ulong expected) => AssertCorrect(CilOpCodes.Shl, a, b, expected);
    }
}