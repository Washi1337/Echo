using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ControlFlow
{
    public class BgtTest : DispatcherTestBase
    {
        public BgtTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Theory]
        [InlineData(CilCode.Bgt, 0, 0, false)]
        [InlineData(CilCode.Bgt_Un, 0, 0, false)]
        [InlineData(CilCode.Bgt, 1, 0, true)]
        [InlineData(CilCode.Bgt_Un, 1, 0, true)]
        [InlineData(CilCode.Bgt, -1, 0, false)]
        [InlineData(CilCode.Bgt_Un, -1, 0, true)]
        public void I4Comparison(CilCode code, int a, int b, bool expectedToTakeBranch)
        {
            var instruction = new CilInstruction(code.ToOpCode(), new CilOffsetLabel(0x1234));
            int expectedOffset = expectedToTakeBranch ? 0x1234 : instruction.Offset + instruction.Size;
            
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I4Value(a));
            stack.Push(new I4Value(b));

            var result = Dispatcher.Execute(ExecutionContext, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0, stack.Size);
            Assert.Equal(expectedOffset, ExecutionContext.ProgramState.ProgramCounter);
        }
    }
}