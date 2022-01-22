using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ControlFlow
{
    public class RetHandlerTest : CilOpCodeHandlerTestBase
    {
        public RetHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void RetFromVoidShouldPopFromCallStack()
        {
            int currentFrameCount = Context.Machine.CallStack.Count;

            var instruction = new CilInstruction(CilOpCodes.Ret);
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(currentFrameCount - 1, Context.Machine.CallStack.Count);
        }
    }
}