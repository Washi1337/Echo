using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arrays
{
    public class LdElemaHandlerTest : CilOpCodeHandlerTestBase
    {
        public LdElemaHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        private long CreateArray(int elementCount)
        {
            var factory = Context.Machine.ValueFactory;
            var elementType = factory.ContextModule.CorLibTypeFactory.Int32;

            long array = Context.Machine.Heap.AllocateSzArray(elementType, elementCount, false);
            var arraySpan = Context.Machine.Heap.GetObjectSpan(array);
            for (int i = 0; i < elementCount; i++)
                arraySpan.SliceArrayElement(factory, elementType, i).Write(100 + i);
            
            return array;
        }

        [Fact]
        public void GetFromNullArrayShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(
                CilOpCodes.Ldelema, 
                Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type));
            
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
        }
        
        [Fact]
        public void GetOutOfRangeShouldThrow()
        {
            long array = CreateArray(10);
            
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(array, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(
                CilOpCodes.Ldelema, 
                Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type));
            
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.IndexOutOfRangeException", exceptionType?.FullName);
        }
        
        [Fact]
        public void GetFromUnknownArrayShouldPushUnknown()
        {
            Context.Machine.UnknownResolver = EmptyUnknownResolver.Instance;
            
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(Context.Machine.ValueFactory.CreateNativeInteger(false), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(1, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(
                CilOpCodes.Ldelema, 
                Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type));

            Assert.True(result.IsSuccess);
            Assert.Single(stack);
            Assert.False(stack.Pop().Contents.AsSpan().IsFullyKnown);
        }
        
        [Fact]
        public void GetFromKnownArrayShouldPushElement()
        {
            long array = CreateArray(10);
            
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(array, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(4, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(
                CilOpCodes.Ldelema, 
                Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type));

            Assert.True(result.IsSuccess);
            Assert.Single(stack);

            long address = stack.Pop().Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit);
            var buffer = new BitVector(32);
            Context.Machine.Memory.Read(address, buffer);
            Assert.Equal(104, buffer.AsSpan().I32);
        }
    }
}