using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ControlFlow
{
    public class BrTrueTest : DispatcherTestBase
    {
        public BrTrueTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Theory]
        [InlineData("1", true)]
        [InlineData("0", false)]
        [InlineData("1000", true)]
        [InlineData("10?0", true)]
        public void Integer32OnStack(string value, bool expectedToHaveTakenBranch)
        {
            var instruction = new CilInstruction(CilOpCodes.Brtrue, new CilOffsetLabel(0x1234));
            int expectedOffset = expectedToHaveTakenBranch ? 0x1234 : instruction.Offset + instruction.Size;
            
            ExecutionContext.ProgramState.Stack.Push(new I4Value(value));
            var result = Dispatcher.Execute(ExecutionContext, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0, ExecutionContext.ProgramState.Stack.Size);
            Assert.Equal(expectedOffset, ExecutionContext.ProgramState.ProgramCounter);
        }
    }
}