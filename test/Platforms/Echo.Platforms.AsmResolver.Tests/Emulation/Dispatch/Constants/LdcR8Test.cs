using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Constants
{
    public class LdcR8Test : DispatcherTestBase
    {
        public LdcR8Test(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void LdcR8()
        {
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldc_R8, 1.23D));
            Assert.True(result.IsSuccess);
            Assert.Equal(new FValue(1.23D), ExecutionContext.ProgramState.Stack.Top);
        }
    }
}