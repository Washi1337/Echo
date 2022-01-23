using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public class BinaryOperatorTestBase : CilOpCodeHandlerTestBase
    {
        public BinaryOperatorTestBase(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        protected void AssertCorrect(CilOpCode code, uint a, uint b, uint expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(new BitVector(a), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(new BitVector(b), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(32, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().U32);
        }

        protected void AssertCorrect(CilOpCode code, uint a, ulong b, ulong expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(new BitVector(a), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(new BitVector(b), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().U64);
        }

        protected void AssertCorrect(CilOpCode code, ulong a, uint b, ulong expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(new BitVector(a), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(new BitVector(b), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().U64);
        }

        protected void AssertCorrect(CilOpCode code, ulong a, ulong b, ulong expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(new BitVector(a), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(new BitVector(b), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().U64);
        }

        protected void AssertCorrect(CilOpCode code, double a, double b, double expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(new BitVector(a), StackSlotTypeHint.Float));
            stack.Push(new StackSlot(new BitVector(b), StackSlotTypeHint.Float));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().F64);
        }
        
    }
}