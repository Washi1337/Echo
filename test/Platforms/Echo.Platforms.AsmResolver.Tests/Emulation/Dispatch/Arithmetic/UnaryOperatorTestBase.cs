using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public abstract class UnaryOperatorTestBase : CilOpCodeHandlerTestBase
    {
        protected UnaryOperatorTestBase(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        protected void AssertCorrect(CilOpCode code, int a, int expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(32, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().I32);
        }

        protected void AssertCorrect(CilOpCode code, long a, long expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().I64);
        }
    }
}