using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Misc
{
    public class PopHandlerTest : CilOpCodeHandlerTestBase
    {

        public PopHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void PopShouldRemoveOneFromStack()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(new BitVector(1337), StackSlotTypeHint.Integer));

            Assert.Single(stack);
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Pop));

            Assert.True(result.IsSuccess);
            Assert.Empty(stack);
        }
    }
}