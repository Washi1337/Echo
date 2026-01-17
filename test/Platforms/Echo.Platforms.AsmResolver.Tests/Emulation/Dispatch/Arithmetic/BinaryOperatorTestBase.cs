using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arithmetic
{
    public abstract class BinaryOperatorTestBase : CilOpCodeHandlerTestBase
    {
        protected BinaryOperatorTestBase(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        protected void AssertCorrect(CilOpCode code, uint a, uint b, bool expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(32, slot.Contents.Count);
            Assert.Equal(expected, !slot.Contents.AsSpan().IsZero);
        }
        
        protected void AssertCorrect(CilOpCode code, uint a, ulong b, bool expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(32, slot.Contents.Count);
            Assert.Equal(expected, !slot.Contents.AsSpan().IsZero);
        }
        
        protected void AssertCorrect(CilOpCode code, ulong a, uint b, bool expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(32, slot.Contents.Count);
            Assert.Equal(expected, !slot.Contents.AsSpan().IsZero);
        }
        
        protected void AssertCorrect(CilOpCode code, ulong a, ulong b, bool expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(32, slot.Contents.Count);
            Assert.Equal(expected, !slot.Contents.AsSpan().IsZero);
        }

        protected void AssertCorrect(CilOpCode code, uint a, uint b, uint expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(32, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().U32);
        }

        protected void AssertCorrect(CilOpCode code, int a, int b, int expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(32, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().I32);
        }

        protected void AssertCorrect(CilOpCode code, uint a, ulong b, ulong expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().U64);
        }

        protected void AssertCorrect(CilOpCode code, ulong a, uint b, ulong expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().U64);
        }

        protected void AssertCorrect(CilOpCode code, ulong a, ulong b, ulong expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().U64);
        }
        
        protected void AssertCorrect(CilOpCode code, long a, long b, long expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().I64);
        }

        protected void AssertCorrect(CilOpCode code, int a, long b, long expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().I64);
        }

        protected void AssertCorrect(CilOpCode code, long a, int b, long expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().I64);
        }

        protected void AssertCorrect(CilOpCode code, double a, double b, double expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(a, StackSlotTypeHint.Float));
            stack.Push(new StackSlot(b, StackSlotTypeHint.Float));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code));
            
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(64, slot.Contents.Count);
            Assert.Equal(expected, slot.Contents.AsSpan().F64);
        }
        
    }
}