using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arrays
{
    public class StElemHandlerTest : CilOpCodeHandlerTestBase
    {
        public StElemHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void WriteOutOfRangeShouldThrow()
        {
            long array = Context.Machine.Heap.AllocateSzArray(Context.Machine.ContextModule.CorLibTypeFactory.Int32, 10, true);
            
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(array, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(1337, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stelem_I4));
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.ToObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.IndexOutOfRangeException", exceptionType?.FullName);
        }

        [Fact]
        public void WriteInt32Element()
        {
            var factory = Context.Machine.ValueFactory;
            var stack = Context.CurrentFrame.EvaluationStack;
            
            var elementType = Context.Machine.ContextModule.CorLibTypeFactory.Int32;
            long array = Context.Machine.Heap.AllocateSzArray(elementType, 10, true);

            Assert.All(Enumerable.Range(0, 10), i =>
            {
                stack.Push(new StackSlot(array, StackSlotTypeHint.Integer));
                stack.Push(new StackSlot(i, StackSlotTypeHint.Integer));
                stack.Push(new StackSlot(100 + i, StackSlotTypeHint.Integer));
                var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stelem_I4));
                
                Assert.True(result.IsSuccess);
                Assert.Empty(stack);
                
                var arraySpan = Context.Machine.Heap.GetObjectSpan(array);
                Assert.Equal(100 + i, arraySpan.SliceArrayElement(factory, elementType, i).I32);
            });
        }
    }
}