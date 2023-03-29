using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arrays
{
    public class LdElemHandlerTest : CilOpCodeHandlerTestBase
    {
        public LdElemHandlerTest(MockModuleFixture fixture)
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
        public void ReadFromNullArrayShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldelem_I4));
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
        }

        [Fact]
        public void ReadOutOfRangeShouldThrow()
        {
            long array = CreateArray(10);
            
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(array, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldelem_I4));
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.IndexOutOfRangeException", exceptionType?.FullName);
        }
        
        [Fact]
        public void ReadInt32Element()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            long array = CreateArray(10);

            Assert.All(Enumerable.Range(0, 10), i =>
            {
                stack.Push(new StackSlot(array, StackSlotTypeHint.Integer));
                stack.Push(new StackSlot(i, StackSlotTypeHint.Integer));
                var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldelem_I4));
                
                Assert.True(result.IsSuccess);
                Assert.Single(stack);
                var value = stack.Pop();
                Assert.Equal(100 + i, value.Contents.AsSpan().I32);
            });
        }
        
        [Fact]
        public void ReadInt32ElementFromUnknownArray()
        {
            Context.Machine.UnknownResolver = EmptyUnknownResolver.Instance;
            
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(Context.Machine.ValueFactory.CreateNativeInteger(false), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));
        
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldelem_I4));
            Assert.True(result.IsSuccess);
            Assert.False(stack.Peek().Contents.AsSpan().IsFullyKnown);
        }

    }
}