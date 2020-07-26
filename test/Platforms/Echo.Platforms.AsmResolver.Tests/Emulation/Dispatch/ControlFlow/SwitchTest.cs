using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ControlFlow
{
    public class SwitchTest : DispatcherTestBase
    {
        public SwitchTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Theory]
        [InlineData(0, 0x1234)]
        [InlineData(1, 0x5678)]
        [InlineData(2, 0x9ABC)]
        [InlineData(3, 0xDEF0)]
        [InlineData(4, null)]
        public void Switch(int index, long? expectedOffset)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(index));

            var instruction = new CilInstruction(CilOpCodes.Switch, new ICilLabel[]
            {
                new CilOffsetLabel(0x1234),
                new CilOffsetLabel(0x5678),
                new CilOffsetLabel(0x9ABC),
                new CilOffsetLabel(0xDEF0),
            });
            expectedOffset ??= instruction.Offset + instruction.Size;
            
            var result = Dispatcher.Execute(ExecutionContext, instruction);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, stack.Size);
            Assert.Equal(expectedOffset, ExecutionContext.ProgramState.ProgramCounter);
        }
    }
}