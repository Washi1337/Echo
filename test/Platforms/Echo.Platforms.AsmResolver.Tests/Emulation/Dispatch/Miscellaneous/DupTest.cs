using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Miscellaneous
{
    public class DupTest : DispatcherTestBase
    {
        public DupTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void DupWithValueType()
        {
            var value = new I4Value(1234);
            ExecutionContext.ProgramState.Stack.Push(value);
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Dup));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(2, ExecutionContext.ProgramState.Stack.Size);
            Assert.All(
                ExecutionContext.ProgramState.Stack.GetAllStackSlots(), 
                v => Assert.Equal(v, value));
        }
    }
}