using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel;

public class LdFldaHandlerTest : CilOpCodeHandlerTestBase
{
    public LdFldaHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void GetInstanceFieldFromClass()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        
        // Obtain class type and field.
        var classType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
        var field = classType.Fields.First(f => f.Name == nameof(SimpleClass.IntField));

        // Allocate object of the class, and set the field to 1337.
        long objectAddress = Context.Machine.Heap.AllocateObject(classType, true);
        long fieldAddress = Context.Machine.ValueFactory.GetFieldAddress(objectAddress, field);
        
        // Push address of object onto stack.
        stack.Push(new StackSlot(objectAddress, StackSlotTypeHint.Integer));

        // Dispatch
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldflda, field));

        // Verify.
        Assert.True(result.IsSuccess);
        Assert.Single(stack);
        Assert.Equal(fieldAddress, stack.Pop().Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit));
    }

    [Fact]
    public void GetInstanceFieldFromStructureByReference()
    {
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
        
        // Push address to struct.
        var stack = Context.CurrentFrame.EvaluationStack;
        long objectAddress = Context.CurrentFrame.GetLocalAddress(0);
        long fieldAddress = Context.Machine.ValueFactory.GetFieldAddress(objectAddress, field);
        
        stack.Push(new StackSlot(objectAddress, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldflda, field));

        // Evaluate.
        Assert.True(result.IsSuccess);
        Assert.Single(stack);
        Assert.Equal(fieldAddress, stack.Pop().Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit));
    }
}