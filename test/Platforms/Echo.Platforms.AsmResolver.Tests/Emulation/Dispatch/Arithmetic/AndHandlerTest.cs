using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class AndHandlerTest : BinaryOperatorTestBase
    {
        public AndHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void AndI4ToI4() => AssertCorrect(CilOpCodes.And, 0x12345678u, 0xff00ff00, 0x12005600);

        [Fact]
        public void AndI4ToI8() => AssertCorrect(CilOpCodes.And, 0x12345678u, 0x1234_5678_ff00ff00ul, 0x12005600ul);

        [Fact]
        public void AndI8ToI4() => AssertCorrect(CilOpCodes.And, 0x1234_5678_ff00ff00ul, 0x12345678u, 0x12005600ul);

        [Fact]
        public void AndI8ToI8()=> AssertCorrect(CilOpCodes.And, 0x1234_5678_9abc_def0ul, 0xff00_ff00_ff00_ff00ul, 0x1200_5600_9a00_de00ul);
    }
}