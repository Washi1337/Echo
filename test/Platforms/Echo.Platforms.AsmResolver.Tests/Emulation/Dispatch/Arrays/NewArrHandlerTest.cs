using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arrays
{
    public class NewArrHandlerTest : CilOpCodeHandlerTestBase
    {

        public NewArrHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void NewInt32ArrayFullyKnownSize()
        {
            var factory = Context.Machine.ValueFactory;
            
            int elementCount = 10;
            var elementType = factory.ContextModule.CorLibTypeFactory.Int32;
            
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(elementCount, StackSlotTypeHint.Integer));

            var instruction = new CilInstruction(CilOpCodes.Newarr, elementType.Type);
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            
            var slot = Assert.Single(Context.CurrentFrame.EvaluationStack);
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            
            var arraySpan = Context.Machine.Heap.GetObjectSpan(slot.Contents);
            Assert.Equal(elementCount, arraySpan.SliceArrayLength(factory).I32);
            for (int i = 0; i < elementCount; i++)
                Assert.Equal(0, arraySpan.SliceArrayElement(factory, elementType, i).I32);
        }

        [Fact]
        public void NewInt32ArrayUnknownSizeShouldPushUnknownPointer()
        {
            var elementType = Context.Machine.ValueFactory.ContextModule.CorLibTypeFactory.Int32;
            
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(
                new BitVector(32, false), 
                StackSlotTypeHint.Integer));

            var instruction = new CilInstruction(CilOpCodes.Newarr, elementType.Type);
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            
            var slot = Assert.Single(Context.CurrentFrame.EvaluationStack);
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            Assert.False(slot.Contents.AsSpan().IsFullyKnown);
        }
    }
}