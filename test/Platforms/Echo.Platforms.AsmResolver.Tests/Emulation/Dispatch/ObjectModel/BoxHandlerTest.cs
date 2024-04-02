using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel;

public class BoxHandlerTest : CilOpCodeHandlerTestBase
{
    public BoxHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void BoxReferenceTypeShouldBeNop()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        var factory = Context.Machine.ValueFactory;

        // Allocate object.
        var type = ModuleFixture.MockModule.CorLibTypeFactory.Object.Type;
        long address = Context.Machine.Heap.AllocateObject(type, true);
        stack.Push(new StackSlot(factory.CreateNativeInteger(address), StackSlotTypeHint.Integer));

        // Box
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Box, type));

        // Same address should be on the stack.
        Assert.True(result.IsSuccess);
        var value = Assert.Single(stack);
        Assert.Equal(address, value.Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit));
    }

    [Fact]
    public void BoxPrimitive()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        var manager = Context.Machine.TypeManager;

        // Push integer.
        var type = ModuleFixture.MockModule.CorLibTypeFactory.Int32.Type;
        stack.Push(new StackSlot(1337, StackSlotTypeHint.Integer));

        // Box.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Box, type));

        // Read box object.
        Assert.True(result.IsSuccess);
        var handle = Assert.Single(stack).Contents.AsObjectHandle(Context.Machine);
        
        Assert.Equal(type, handle.GetMethodTable().Type, SignatureComparer.Default);
        Assert.Equal(1337, Context.Machine.Heap.GetObjectSpan(handle.Address).SliceObjectData(manager).I32);
    }

    [Fact]
    public void BoxUserValueType()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        var manager = Context.Machine.TypeManager;

        // Look up metadata.
        var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
        var fieldX = type.Fields.First(f => f.Name == nameof(SimpleStruct.X));
        var fieldY = type.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
        var fieldZ = type.Fields.First(f => f.Name == nameof(SimpleStruct.Z));
        
        // Create new SimpleStruct { X=1337, Y=1338, Z=1339 }; 
        var value = Context.Machine.ValueFactory.CreateValue(type.ToTypeSignature(true), false);
        value.AsSpan().SliceStructField(manager, fieldX).Write(1337);
        value.AsSpan().SliceStructField(manager, fieldY).Write(1338);
        value.AsSpan().SliceStructField(manager, fieldZ).Write(1339);
        
        // Push it.
        stack.Push(new StackSlot(value, StackSlotTypeHint.Structure));

        // Box.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Box, type));

        // Read box object.
        Assert.True(result.IsSuccess);
        var handle = Assert.Single(stack).Contents.AsObjectHandle(Context.Machine);
        
        Assert.Equal(type, handle.GetMethodTable().Type, SignatureComparer.Default);
        var boxObjectSpan = Context.Machine.Heap.GetObjectSpan(handle.Address);
        Assert.Equal(1337, boxObjectSpan.SliceObjectField(manager, fieldX).I32);
        Assert.Equal(1338, boxObjectSpan.SliceObjectField(manager, fieldY).I32);
        Assert.Equal(1339, boxObjectSpan.SliceObjectField(manager, fieldZ).I32);
    }
}