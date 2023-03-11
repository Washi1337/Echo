using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel;

public class LdFldHandlerTest : CilOpCodeHandlerTestBase
{

    public LdFldHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void ReadInstanceFieldFromClass()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        var factory = Context.Machine.ValueFactory;
        
        // Obtain class type and field.
        var classType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
        var field = classType.Fields.First(f => f.Name == nameof(SimpleClass.IntField));

        // Allocate object of the class, and set the field to 1337.
        long address = Context.Machine.Heap.AllocateObject(classType, true);
        var objectSpan = Context.Machine.Heap.GetObjectSpan(address);
        objectSpan.SliceObjectField(factory, field).Write(1337);
        
        // Push address of object onto stack.
        stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));

        // Dispatch
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldfld, field));

        // Verify.
        Assert.True(result.IsSuccess);
        Assert.Single(stack);
        Assert.Equal(1337, stack.Pop().Contents.AsSpan().I32);
    }

    [Fact]
    public void ReadInstanceFieldFromStructureByValue()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        var factory = Context.Machine.ValueFactory;

        // Obtain struct type and field.
        var structType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
        var field = structType.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
        
        // Create instance of struct, and set field to 1337.
        var objectSpan = factory.CreateValue(structType.ToTypeSignature(), true);
        objectSpan.AsSpan().SliceStructField(factory, field).Write(1337);
        
        // Push struct onto stack
        stack.Push(new StackSlot(objectSpan, StackSlotTypeHint.Structure));

        // Dispatch
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldfld, field));

        // Verify.
        Assert.True(result.IsSuccess);
        Assert.Single(stack);
        Assert.Equal(1337, stack.Pop().Contents.AsSpan().I32);
    }

    [Fact]
    public void ReadInstanceFieldFromStructureByReference()
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

        Context.Machine.CallStack.Push(method);

        // Initialize variable with field set to 1337. 
        var instance = factory.CreateValue(structType.ToTypeSignature(), true);
        instance.AsSpan().SliceStructField(factory, field).Write(1337);
        Context.CurrentFrame.WriteLocal(0, instance);
        
        // Push address to struct.
        var stack = Context.CurrentFrame.EvaluationStack;
        long address = Context.CurrentFrame.GetLocalAddress(0);
        stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldfld, field));

        // Evaluate.
        Assert.True(result.IsSuccess);
        Assert.Single(stack);
        Assert.Equal(1337, stack.Pop().Contents.AsSpan().I32);
    }
}