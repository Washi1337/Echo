using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel;

public class StFldHandlerTest : CilOpCodeHandlerTestBase
{
    public StFldHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void WriteStaticField()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        
        // Obtain field.
        var field = ModuleFixture.MockModule
            .TopLevelTypes.First(t => t.Name == nameof(SimpleClass))
            .Fields.First(f => f.Name == nameof(SimpleClass.StaticIntField));
        
        // Push null and value.
        stack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(1337, StackSlotTypeHint.Integer));
        
        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stfld, field));
        
        // Verify.
        Assert.True(result.IsSuccess);
        Assert.Empty(stack);
        Assert.Equal(1337, Context.Machine.StaticFields.GetFieldSpan(field).I32);
    }
    
    [Fact]
    public void WriteInstanceFieldFromClass()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        var factory = Context.Machine.ValueFactory;
        
        // Obtain class type and field.
        var classType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
        var field = classType.Fields.First(f => f.Name == nameof(SimpleClass.IntField));

        // Allocate object of the class.
        long address = Context.Machine.Heap.AllocateObject(classType, true);
        var objectSpan = Context.Machine.Heap.GetObjectSpan(address);
        
        // Push address of object onto stack.
        stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(1337, StackSlotTypeHint.Integer));

        // Dispatch
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stfld, field));

        // Verify.
        Assert.True(result.IsSuccess);
        Assert.Empty(stack);
        Assert.Equal(1337, objectSpan.SliceObjectField(factory, field).I32);
    }
    
    [Fact]
    public void WriteInstanceFieldFromStructureByReference()
    {
        var factory = Context.Machine.ValueFactory;
        
        // Obtain struct type and field.
        var module = ModuleFixture.MockModule;
        var structType = module.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
        var field = structType.Fields.First(f => f.Name == nameof(SimpleStruct.Y));

        // Create method with a struct local so that we can take the address of an instance.
        var method = new MethodDefinition("Dummy", MethodAttributes.Static,
            MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));
        method.CilMethodBody = new CilMethodBody(method)
        {
            LocalVariables = {new CilLocalVariable(structType.ToTypeSignature())}
        };

        Context.Thread.CallStack.Push(method);
        
        // Push address to struct.
        var stack = Context.CurrentFrame.EvaluationStack;
        long address = Context.CurrentFrame.GetLocalAddress(0);
        stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(1337, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stfld, field));

        // Evaluate.
        Assert.True(result.IsSuccess);
        Assert.Empty(stack);
        
        var instance = factory.CreateValue(structType.ToTypeSignature(), true);
        Context.CurrentFrame.ReadLocal(0, instance);
        Assert.Equal(1337, instance.AsSpan().SliceStructField(factory, field).I32);
    }
}