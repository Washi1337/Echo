using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ControlFlow
{
    public class BrHandlerTest : CilOpCodeHandlerTestBase
    {
        public BrHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void BranchShouldUpdateProgramCounter()
        {
            var instruction = new CilInstruction(CilOpCodes.Br, new CilOffsetLabel(0x1337));
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0x1337, Context.CurrentFrame.ProgramCounter);
        }
    }
}