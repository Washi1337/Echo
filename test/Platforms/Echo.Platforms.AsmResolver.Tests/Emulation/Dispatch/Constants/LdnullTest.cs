using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Constants
{
    public class LdnullTest : DispatcherTestBase
    {
        public LdnullTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Fact]
        public void LdNull()
        {
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldnull));
            Assert.True(result.IsSuccess);
            Assert.Equal(OValue.Null(false), ExecutionContext.ProgramState.Stack.Top);
        }
    }
}