using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class CeqHandlerTest : BinaryOperatorTestBase
    {
        public CeqHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Theory]
        [InlineData(1000u, 1000u)]
        [InlineData(1000u, 2000u)]
        public void I4ToI4(uint a, uint b) => AssertCorrect(CilOpCodes.Ceq, a, b, a == b);

        [Theory]
        [InlineData(1000u, 1000ul)]
        [InlineData(1000u, 2000ul)]
        public void I4ToI8(uint a, ulong b) => AssertCorrect(CilOpCodes.Ceq, a, b, a == b);

        [Theory]
        [InlineData(1000ul, 1000u)]
        [InlineData(1000ul, 2000u)]
        public void I8ToI4(ulong a, uint b) => AssertCorrect(CilOpCodes.Ceq, a, b, a == b);

        [Theory]
        [InlineData(1000ul, 1000ul)]
        [InlineData(1000ul, 2000ul)]
        public void I8ToI8(ulong a, ulong b) => AssertCorrect(CilOpCodes.Ceq, a, b, a == b);
    }

}