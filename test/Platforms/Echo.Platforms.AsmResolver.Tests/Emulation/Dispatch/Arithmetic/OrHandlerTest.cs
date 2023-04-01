using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class OrHandlerTest : BinaryOperatorTestBase
    {
        public OrHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void OrI4ToI4() => AssertCorrect(CilOpCodes.Or, 0x22220000u, 0x00001111, 0x22221111);

        [Fact]
        public void OrI4ToI8() => AssertCorrect(CilOpCodes.Or, 0x22222222u, 0x11111111_00000000ul, 0x11111111_22222222ul);

        [Fact]
        public void OrI8ToI4() => AssertCorrect(CilOpCodes.Or, 0x11111111_00000000ul, 0x22222222u, 0x11111111_22222222ul);

        [Fact]
        public void OrI8ToI8() => AssertCorrect(CilOpCodes.Or, 0x11111111_00000000ul, 0x00000000_22222222ul, 0x11111111_22222222ul);
    }
}