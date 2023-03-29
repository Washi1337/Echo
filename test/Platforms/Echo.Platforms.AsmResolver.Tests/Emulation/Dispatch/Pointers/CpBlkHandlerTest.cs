using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers;

public class CpBlkHandlerTest : CilOpCodeHandlerTestBase
{
    public CpBlkHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }
    
    [Fact]
    public void CopyFromNullShouldThrow()
    {
        var stack = Context.CurrentFrame.EvaluationStack;

        // Map some destination.
        long destinationAddress = 0x0600_0000;
        Context.Machine.Memory.Map(destinationAddress, new BasicMemorySpace(10, false));
        
        // Push null, a value, and a size.
        stack.Push(new StackSlot(destinationAddress, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(0ul, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Cpblk));

        // Verify
        Assert.False(result.IsSuccess);
        var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
        Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
    }
    
    [Fact]
    public void CopyToNullShouldThrow()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        
        // Map some source.
        long sourceAddress = 0x0600_0000;
        Context.Machine.Memory.Map(sourceAddress, new BasicMemorySpace(10, false));
        
        // Push null, a value, and a size.
        stack.Push(new StackSlot(0ul, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(sourceAddress, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Cpblk));

        // Verify
        Assert.False(result.IsSuccess);
        var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
        Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
    }
    
    [Fact]
    public void CopyValidAddresses()
    {
        byte[] data = {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        };

        var stack = Context.CurrentFrame.EvaluationStack;

        // Map some source.
        long sourceAddress = 0x0600_0000;
        long destinationAddress = 0x0700_0000;
        Context.Machine.Memory.Map(sourceAddress, new BasicMemorySpace(data));
        Context.Machine.Memory.Map(destinationAddress, new BasicMemorySpace(10, false));
        
        // Push null, a value, and a size.
        stack.Push(new StackSlot(destinationAddress, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(sourceAddress, StackSlotTypeHint.Integer));
        stack.Push(new StackSlot(10, StackSlotTypeHint.Integer));

        // Dispatch.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Cpblk));

        // Verify
        Assert.True(result.IsSuccess);

        var buffer = new BitVector(10 * 8, false);
        Context.Machine.Memory.Read(destinationAddress, buffer);
        Assert.Equal(data, buffer.Bits);
    }
}