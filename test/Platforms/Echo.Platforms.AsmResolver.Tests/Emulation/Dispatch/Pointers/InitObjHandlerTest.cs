using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers;

public class InitObjHandlerTest : CilOpCodeHandlerTestBase
{
    public InitObjHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void InitOnNullShouldThrow()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));
        
        // Push null.
        stack.Push(new StackSlot(0ul, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Initobj, type));

        // Verify
        Assert.False(result.IsSuccess);
        var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
        Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
    }

    [Fact]
    public void InitOnPointerShouldSetAllToZeroes()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));

        // Map some mock memory.
        long address = 0x0600_0000;
        Context.Machine.Memory.Map(address, new BasicMemorySpace(1000, false));
        
        // Push address.
        stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Initobj, type));

        // Verify.
        Assert.True(result.IsSuccess);
        var buffer = Context.Machine.ValueFactory.CreateValue(type.ToTypeSignature(), false);
        Context.Machine.Memory.Read(address, buffer);
        Assert.True(buffer.AsSpan().IsZero.ToBoolean());
    }
}