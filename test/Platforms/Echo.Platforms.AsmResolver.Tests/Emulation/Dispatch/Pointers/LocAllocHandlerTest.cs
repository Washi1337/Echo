using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers;

public class LocAllocHandlerTest : CilOpCodeHandlerTestBase
{
    public LocAllocHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void PushValidAddressWithinFrame()
    {
        var frame = Context.CurrentFrame;

        frame.EvaluationStack.Push(new StackSlot(10, StackSlotTypeHint.Integer));
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Localloc));

        Assert.True(result.IsSuccess);
        var value = Assert.Single(frame.EvaluationStack);
        Assert.True(frame.IsValidAddress(value.Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit)));
    }
}