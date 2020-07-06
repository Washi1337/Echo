using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Constants
{
    public class LdcI8Test : DispatcherTestBase
    {
        public LdcI8Test(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void LdcI8()
        {
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldc_I8, 0x0123456789ABCDEFL));
            Assert.True(result.IsSuccess);
            Assert.Equal(new I8Value(0x0123456789ABCDEFL), ExecutionContext.ProgramState.Stack.Top);
        }
    }
}