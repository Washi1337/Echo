using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Miscellaneous
{
    public class PopTest : DispatcherTestBase
    {
        public PopTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void Pop()
        {
            ExecutionContext.ProgramState.Stack.Push(new UnknownValue());
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Pop));
            Assert.True(result.IsSuccess);
            Assert.Equal(0, ExecutionContext.ProgramState.Stack.Size);
        }
    }
}