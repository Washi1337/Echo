using System;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class CastHandlerTest : CilOpCodeHandlerTestBase
    {
        public CastHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void UnboxedPointerShouldPointToStructData()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var factory = Context.Machine.ValueFactory;
            
            var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
            var fieldX = type.Fields.First(f => f.Name == nameof(SimpleStruct.X));
            var fieldY = type.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
            var fieldZ = type.Fields.First(f => f.Name == nameof(SimpleStruct.Z));

            long address = Context.Machine.Heap.AllocateObject(type, false);
            var objectSpan = Context.Machine.Heap.GetObjectSpan(address);
            objectSpan.SliceObjectField(factory, fieldX).Write(1337);
            objectSpan.SliceObjectField(factory, fieldY).Write(1338);
            objectSpan.SliceObjectField(factory, fieldZ).Write(1339);

            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Unbox, type));

            Assert.True(result.IsSuccess);
            long dataAddress = Assert.Single(stack).Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit);

            var buffer = factory.CreateValue(type.ToTypeSignature(true), false).AsSpan();
            Context.Machine.Memory.Read(dataAddress, buffer);
            Assert.Equal(1337, buffer.SliceStructField(factory, fieldX).I32);
            Assert.Equal(1338, buffer.SliceStructField(factory, fieldY).I32);
            Assert.Equal(1339, buffer.SliceStructField(factory, fieldZ).I32);
        }

        [Theory]
        [InlineData(CilCode.Unbox)]
        [InlineData(CilCode.Unbox_Any)]
        public void UnboxUnmatchedTypeShouldThrow(CilCode code)
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var factory = Context.Machine.ValueFactory;
            
            var actualType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
            var targetType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(Int32Enum));
            long address = Context.Machine.Heap.AllocateObject(actualType, false);
            
            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode(), targetType));
            
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.InvalidCastException", exceptionType.FullName);
        }

        [Fact]
        public void CastClassUnmatchedTypeShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var factory = Context.Machine.ValueFactory;
            
            var actualType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
            var targetType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            long address = Context.Machine.Heap.AllocateObject(actualType, false);
            
            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Castclass, targetType));
            
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.InvalidCastException", exceptionType.FullName);
        }

        [Fact]
        public void IsInstUnmatchedTypeShouldPushNull()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var factory = Context.Machine.ValueFactory;
            
            var actualType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
            var targetType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            long address = Context.Machine.Heap.AllocateObject(actualType, false);
            
            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Isinst, targetType));
            
            Assert.True(result.IsSuccess);
            var value = Assert.Single(stack);
            Assert.True(value.Contents.AsSpan().IsZero.ToBoolean());
        }

        [Fact]
        public void UnboxAnyShouldPushStructData()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var factory = Context.Machine.ValueFactory;
            
            var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
            var fieldX = type.Fields.First(f => f.Name == nameof(SimpleStruct.X));
            var fieldY = type.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
            var fieldZ = type.Fields.First(f => f.Name == nameof(SimpleStruct.Z));

            long address = Context.Machine.Heap.AllocateObject(type, false);
            var objectSpan = Context.Machine.Heap.GetObjectSpan(address);
            objectSpan.SliceObjectField(factory, fieldX).Write(1337);
            objectSpan.SliceObjectField(factory, fieldY).Write(1338);
            objectSpan.SliceObjectField(factory, fieldZ).Write(1339);

            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Unbox_Any, type));

            Assert.True(result.IsSuccess);
            var buffer = Assert.Single(stack).Contents.AsSpan();
            Assert.Equal(1337, buffer.SliceStructField(factory, fieldX).I32);
            Assert.Equal(1338, buffer.SliceStructField(factory, fieldY).I32);
            Assert.Equal(1339, buffer.SliceStructField(factory, fieldZ).I32);
        }

        [Theory]
        [InlineData(CilCode.Castclass, typeof(SimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Isinst, typeof(SimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Unbox_Any, typeof(SimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Castclass, typeof(DerivedSimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Isinst, typeof(DerivedSimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Unbox_Any, typeof(DerivedSimpleClass), typeof(SimpleClass))]
        public void CastMatchedTypeShouldPushSameObject(CilCode code, Type t1, Type t2)
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var factory = Context.Machine.ValueFactory;
            
            var actualType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == t1.Name);
            var targetType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == t2.Name);
            long address = Context.Machine.Heap.AllocateObject(actualType, false);
            
            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode(), targetType));
            
            Assert.True(result.IsSuccess);
            var value = Assert.Single(stack);
            Assert.Equal(address, value.Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit));
        }
    }
}