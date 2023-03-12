using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ControlFlow;

public class SwitchHandlerTest : CilOpCodeHandlerTestBase
{
    public SwitchHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Theory]
    [InlineData(10)]
    [InlineData(-1)]
    public void SwitchToDefault(int index)
    {
        var stack = Context.CurrentFrame.EvaluationStack;

        stack.Push(new StackSlot(index, StackSlotTypeHint.Integer));

        var instruction = new CilInstruction(CilOpCodes.Switch, new[]
        {
            new CilOffsetLabel(1337),
            new CilOffsetLabel(1338),
            new CilOffsetLabel(1339),
        });
        var result = Dispatcher.Dispatch(Context, instruction);

        Assert.True(result.IsSuccess);
        Assert.Equal(instruction.Offset + instruction.Size, Context.CurrentFrame.ProgramCounter);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void SwitchToSpecificCaseLabel(int index)
    {
        var stack = Context.CurrentFrame.EvaluationStack;

        stack.Push(new StackSlot(index, StackSlotTypeHint.Integer));

        var instruction = new CilInstruction(CilOpCodes.Switch, new[]
        {
            new CilOffsetLabel(1337),
            new CilOffsetLabel(1338),
            new CilOffsetLabel(1339),
        });
        var result = Dispatcher.Dispatch(Context, instruction);

        Assert.True(result.IsSuccess);
        Assert.Equal(1337 + index, Context.CurrentFrame.ProgramCounter);
    }
}