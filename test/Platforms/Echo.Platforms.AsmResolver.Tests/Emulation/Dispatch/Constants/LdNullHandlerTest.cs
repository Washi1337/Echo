using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Constants
{
    public class LdNullHandlerTest : CilOpCodeHandlerTestBase
    {
        public LdNullHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void ShouldPushNull()
        {
            Assert.Empty(Context.CurrentFrame.EvaluationStack);
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldnull));
            
            Assert.True(result.IsSuccess);
            
            var value = Assert.Single(Context.CurrentFrame.EvaluationStack);
            Assert.Equal(StackSlotTypeHint.Integer, value.TypeHint);
            Assert.True(value.Contents.AsSpan().IsZero.ToBoolean());
        }
    }
}