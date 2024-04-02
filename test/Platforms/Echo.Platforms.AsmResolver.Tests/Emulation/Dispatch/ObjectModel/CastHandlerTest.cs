using System;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
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
            var manager = Context.Machine.TypeManager;
            var factory = Context.Machine.ValueFactory;
            
            var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
            var fieldX = type.Fields.First(f => f.Name == nameof(SimpleStruct.X));
            var fieldY = type.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
            var fieldZ = type.Fields.First(f => f.Name == nameof(SimpleStruct.Z));

            long address = Context.Machine.Heap.AllocateObject(type, false);
            var objectSpan = Context.Machine.Heap.GetObjectSpan(address);
            objectSpan.SliceObjectField(manager, fieldX).Write(1337);
            objectSpan.SliceObjectField(manager, fieldY).Write(1338);
            objectSpan.SliceObjectField(manager, fieldZ).Write(1339);

            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Unbox, type));

            Assert.True(result.IsSuccess);
            long dataAddress = Assert.Single(stack).Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit);

            var buffer = factory.CreateValue(type.ToTypeSignature(true), false).AsSpan();
            Context.Machine.Memory.Read(dataAddress, buffer);
            Assert.Equal(1337, buffer.SliceStructField(manager, fieldX).I32);
            Assert.Equal(1338, buffer.SliceStructField(manager, fieldY).I32);
            Assert.Equal(1339, buffer.SliceStructField(manager, fieldZ).I32);
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
            var exceptionType = result.ExceptionObject.GetMethodTable().Type;
            Assert.Equal("System.InvalidCastException", exceptionType.FullName);
        }

        [Theory]
        [InlineData(CilCode.Castclass)]
        [InlineData(CilCode.Isinst)]
        [InlineData(CilCode.Unbox_Any)]
        public void CastRefTypeOnNullShouldPushNull(CilCode code)
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var factory = Context.Machine.ValueFactory;
            var targetType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            
            stack.Push(new StackSlot(factory.CreateNull(), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode(), targetType));
            
            Assert.True(result.IsSuccess);
            var value = Assert.Single(stack);
            Assert.True(value.Contents.AsSpan().IsZero.ToBoolean());
        }
        
        [Theory]
        [InlineData(CilCode.Castclass)]
        [InlineData(CilCode.Unbox_Any)]
        public void CastOnUnmatchedRefTypeShouldThrow(CilCode code)
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var factory = Context.Machine.ValueFactory;
            
            var actualType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
            var targetType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            long address = Context.Machine.Heap.AllocateObject(actualType, false);
            
            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode(), targetType));
            
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionObject.GetMethodTable().Type;
            Assert.Equal("System.InvalidCastException", exceptionType.FullName);
        }

        [Fact]
        public void IsInstOnUnmatchedRefTypeShouldPushNull()
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
            var manager = Context.Machine.TypeManager;
            var factory = Context.Machine.ValueFactory;
            
            var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
            var fieldX = type.Fields.First(f => f.Name == nameof(SimpleStruct.X));
            var fieldY = type.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
            var fieldZ = type.Fields.First(f => f.Name == nameof(SimpleStruct.Z));

            long address = Context.Machine.Heap.AllocateObject(type, false);
            var objectSpan = Context.Machine.Heap.GetObjectSpan(address);
            objectSpan.SliceObjectField(manager, fieldX).Write(1337);
            objectSpan.SliceObjectField(manager, fieldY).Write(1338);
            objectSpan.SliceObjectField(manager, fieldZ).Write(1339);

            stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Unbox_Any, type));

            Assert.True(result.IsSuccess);
            var buffer = Assert.Single(stack).Contents.AsSpan();
            Assert.Equal(1337, buffer.SliceStructField(manager, fieldX).I32);
            Assert.Equal(1338, buffer.SliceStructField(manager, fieldY).I32);
            Assert.Equal(1339, buffer.SliceStructField(manager, fieldZ).I32);
        }

        [Theory]
        [InlineData(CilCode.Castclass, typeof(SimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Isinst, typeof(SimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Unbox_Any, typeof(SimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Castclass, typeof(DerivedSimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Isinst, typeof(DerivedSimpleClass), typeof(SimpleClass))]
        [InlineData(CilCode.Unbox_Any, typeof(DerivedSimpleClass), typeof(SimpleClass))]
        public void CastMatchedRefTypeShouldPushSameObject(CilCode code, Type t1, Type t2)
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

        [Fact]
        public void CastGenericTypeReturnsInt16()
        {
            var vm = Context.Machine;

            var genericMethod = ModuleFixture.MockModule.TopLevelTypes
                .First(t => t.Name == nameof(SimpleClass))
                .Methods
                .First(m => m.Name == nameof(SimpleClass.GenericMethod));

            var methodSpecification = genericMethod.MakeGenericInstanceMethod(ModuleFixture.MockModule.CorLibTypeFactory.Int16);

            Context.Thread.CallStack.Push(methodSpecification);

            var value = 32000;

            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(value, StackSlotTypeHint.Integer));

            var returnValue = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Box,
                new GenericParameterSignature(GenericParameterType.Type, 0).ToTypeDefOrRef()));

            Assert.True(returnValue.IsSuccess);
            var result = Assert.Single(stack);

            Assert.Equal(value, result.Contents.AsSpan().I16);

        }

        [Fact]
        public void CastGenericTypeReturnsInt32()
        {
            var vm = Context.Machine;

            var genericMethod = ModuleFixture.MockModule.TopLevelTypes
                .First(t => t.Name == nameof(SimpleClass))
                .Methods
                .First(m => m.Name == nameof(SimpleClass.GenericMethod));

            var methodSpecification = genericMethod.MakeGenericInstanceMethod(ModuleFixture.MockModule.CorLibTypeFactory.Int32);

            Context.Thread.CallStack.Push(methodSpecification);

            var value = 214000000;

            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(value, StackSlotTypeHint.Integer));

            var returnValue = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Box,
                new GenericParameterSignature(GenericParameterType.Type, 0).ToTypeDefOrRef()));

            Assert.True(returnValue.IsSuccess);
            var result = Assert.Single(stack);

            Assert.Equal(value, result.Contents.AsSpan().I32);
        }

        [Fact]
        public void CastGenericTypeReturnsInt64()
        {
            var vm = Context.Machine;

            var genericMethod = ModuleFixture.MockModule.TopLevelTypes
                .First(t => t.Name == nameof(SimpleClass))
                .Methods
                .First(m => m.Name == nameof(SimpleClass.GenericMethod));

            var methodSpecification = genericMethod.MakeGenericInstanceMethod(ModuleFixture.MockModule.CorLibTypeFactory.Int64);

            Context.Thread.CallStack.Push(methodSpecification);

            var value = 922000000000000000L;

            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(value, StackSlotTypeHint.Integer));

            var returnValue = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Box,
                new GenericParameterSignature(GenericParameterType.Type, 0).ToTypeDefOrRef()));

            Assert.True(returnValue.IsSuccess);
            var result = Assert.Single(stack);

            Assert.Equal(value, result.Contents.AsSpan().I64);
        }
    }
} 