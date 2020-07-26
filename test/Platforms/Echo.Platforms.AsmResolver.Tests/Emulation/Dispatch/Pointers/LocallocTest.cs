using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class LocallocTest : DispatcherTestBase
    {
        public LocallocTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Fact]
        public void AllocateTest()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I4Value(16));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Localloc));
            
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<PointerValue>(stack.Top);
        }
        
    }
}