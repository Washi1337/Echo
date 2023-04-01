using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class CgtHandlerTest : BinaryOperatorTestBase
    {
        public CgtHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Theory]
        [InlineData(1000, 500)]
        [InlineData(1000, 1000)]
        [InlineData(1000, 2000)]
        [InlineData(-128, 128)]
        public void I4ToI4Signed(int a, int b) => AssertCorrect(CilOpCodes.Cgt, unchecked((uint) a),  unchecked((uint) b), a > b);
        
        [Theory]
        [InlineData(1000u, 500u)]
        [InlineData(1000u, 1000u)]
        [InlineData(1000u, 2000u)]
        [InlineData(unchecked((uint) -128), 128u)]
        public void I4ToI4Unsigned(uint a, uint b) => AssertCorrect(CilOpCodes.Cgt_Un, a, b, a > b);

        [Theory]
        [InlineData(1000u, 500ul)]
        [InlineData(1000u, 1000ul)]
        [InlineData(1000u, 2000ul)]
        public void I4ToI8(uint a, ulong b) => AssertCorrect(CilOpCodes.Cgt_Un, a, b, a > b);

        [Theory]
        [InlineData(1000ul, 500u)]
        [InlineData(1000ul, 1000u)]
        [InlineData(1000ul, 2000u)]
        public void I8ToI4(ulong a, uint b) => AssertCorrect(CilOpCodes.Cgt_Un, a, b, a > b);

        [Theory]
        [InlineData(1000ul, 500ul)]
        [InlineData(1000ul, 1000ul)]
        [InlineData(1000ul, 2000ul)]
        public void I8ToI8(ulong a, ulong b) => AssertCorrect(CilOpCodes.Cgt_Un, a, b, a > b);
    }
}