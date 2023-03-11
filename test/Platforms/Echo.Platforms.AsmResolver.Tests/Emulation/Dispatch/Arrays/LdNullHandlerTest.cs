using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arrays
{
    public class LdNullHandlerTest : CilOpCodeHandlerTestBase
    {

        public LdNullHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void NullArrayShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldlen));
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer?.AsSpan().GetObjectPointerType(Context.Machine);
            Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
        }
        
        [Fact]
        public void UnknownArrayShouldPushUnknown()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(Context.Machine.ValueFactory.CreateNativeInteger(false), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldlen));
            Assert.True(result.IsSuccess);
            Assert.Single(stack);
            Assert.False(stack.Pop().Contents.AsSpan().IsFullyKnown);
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void Int32Array(int length)
        {
            long array = Context.Machine.Heap.AllocateSzArray(Context.Machine.ContextModule.CorLibTypeFactory.Int32, length, true);
            
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(array, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldlen));
            Assert.True(result.IsSuccess);
            Assert.Single(stack);
            Assert.Equal(length, stack.Pop().Contents.AsSpan().I32);
        }
    }
}