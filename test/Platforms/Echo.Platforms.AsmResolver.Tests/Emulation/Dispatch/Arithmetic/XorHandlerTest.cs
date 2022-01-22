using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class XorHandlerTest : BinaryOperatorTestBase
    {
        public XorHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void XorI4ToI4() => AssertCorrect(CilOpCodes.Xor, 0x22220000u, 0x00001111, 0x22221111);

        [Fact]
        public void XorI4ToI8() => AssertCorrect(CilOpCodes.Xor, 0x22222222u, 0x11111111_00000000ul, 0x11111111_22222222ul);

        [Fact]
        public void XorI8ToI4() => AssertCorrect(CilOpCodes.Xor, 0x11111111_00000000ul, 0x22222222u, 0x11111111_22222222ul);

        [Fact]
        public void XorI8ToI8()=> AssertCorrect(CilOpCodes.Xor, 0x11111111_00000000ul, 0x00000000_22222222ul, 0x11111111_22222222ul);
    }
}