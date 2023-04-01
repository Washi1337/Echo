using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Misc
{
    public class NopHandlerTest : CilOpCodeHandlerTestBase
    {
        public NopHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void NopShouldMoveToNextInstruction()
        {
            int currentPc = Context.CurrentFrame.ProgramCounter;

            var instruction = new CilInstruction(CilOpCodes.Nop);
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(currentPc + instruction.Size, Context.CurrentFrame.ProgramCounter);
        }
    }
}