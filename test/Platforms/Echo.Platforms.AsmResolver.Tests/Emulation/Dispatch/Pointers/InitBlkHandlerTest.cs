using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers;

public class InitBlkHandlerTest : CilOpCodeHandlerTestBase
{
    public InitBlkHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }
    
    [Fact]
    public void InitOnNullShouldThrow()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        
        // Push null, a value, and a size.
        stack.Push(new StackSlot(0ul, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(0xab, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Initblk));

        // Verify
        Assert.False(result.IsSuccess);
        var exceptionType = result.ExceptionPointer!.ToObjectHandle(Context.Machine).GetObjectType();
        Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
    }

    [Fact]
    public void InitOnPointerShouldSetAllToZeroes()
    {
        var stack = Context.CurrentFrame.EvaluationStack;

        // Map some mock memory.
        long address = 0x0600_0000;
        Context.Machine.Memory.Map(address, new BasicMemorySpace(1000, false));
        
        // Push address.
        stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(0xab, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Initblk));

        // Verify.
        Assert.True(result.IsSuccess);
        var buffer = new BitVector(10);
        Context.Machine.Memory.Read(address, buffer);
        Assert.All(buffer.Bits, x => Assert.Equal(0xab, x));
    }
}