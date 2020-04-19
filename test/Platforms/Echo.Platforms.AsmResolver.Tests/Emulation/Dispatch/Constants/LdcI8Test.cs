using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
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
        public void LdcR8()
        {
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldc_R8, 1.23D));
            Assert.True(result.IsSuccess);
            Assert.Equal(new Float64Value(1.23D), ExecutionContext.ProgramState.Stack.Top);
        }
    }
}