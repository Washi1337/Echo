using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class ShrHandlerTest : BinaryOperatorTestBase
    {
        public ShrHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Theory]
        [InlineData(1, 0, 1)]
        [InlineData(1, 4, 1 >> 4)]
        [InlineData(-1, 4, -1 >> 4)]
        public void ShiftInt32(int a, int b, int expected) => AssertCorrect(CilOpCodes.Shr, a, b, expected);

        [Theory]
        [InlineData(1, 0, 1)]
        [InlineData(1, 4, 1 >> 4)]
        [InlineData(uint.MaxValue, 4, uint.MaxValue >> 4)]
        public void ShiftUInt32(uint a, uint b, uint expected) => AssertCorrect(CilOpCodes.Shr_Un, a, b, expected);
    }
}