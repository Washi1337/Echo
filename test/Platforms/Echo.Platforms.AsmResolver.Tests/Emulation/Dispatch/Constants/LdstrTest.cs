using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Constants
{
    public class LdstrTest : DispatcherTestBase
    {
        public LdstrTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Fact]
        public void LdstrShouldPushStringObjectReference()
        {
            const string stringLiteral = "Hello, world!";

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldstr, stringLiteral));

            Assert.True(result.IsSuccess);
            
            var top = ExecutionContext.ProgramState.Stack.Top;
            var oValue = Assert.IsAssignableFrom<OValue>(top);
            var stringValue = Assert.IsAssignableFrom<StringValue>(oValue.ReferencedObject);
            Assert.Equal(stringLiteral, stringValue.ToString());
        }
    }
}