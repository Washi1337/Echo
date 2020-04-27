using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Constants
{
    public class LdcI4Test : DispatcherTestBase
    {
        public LdcI4Test(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void LdcI4()
        {
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldc_I4, 1234));
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(1234), ExecutionContext.ProgramState.Stack.Top);
        }

        [Fact]
        public void LdcI4S()
        {
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldc_I4_S, (sbyte) -128));
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(-128), ExecutionContext.ProgramState.Stack.Top);
        }

        [Theory]
        [InlineData(CilCode.Ldc_I4_0, 0)]
        [InlineData(CilCode.Ldc_I4_1, 1)]
        [InlineData(CilCode.Ldc_I4_2, 2)]
        [InlineData(CilCode.Ldc_I4_3, 3)]
        [InlineData(CilCode.Ldc_I4_4, 4)]
        [InlineData(CilCode.Ldc_I4_5, 5)]
        [InlineData(CilCode.Ldc_I4_6, 6)]
        [InlineData(CilCode.Ldc_I4_7, 7)]
        [InlineData(CilCode.Ldc_I4_8, 8)]
        [InlineData(CilCode.Ldc_I4_M1, -1)]
        public void LdcI4Macro(CilCode code, int expected)
        {
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(expected), ExecutionContext.ProgramState.Stack.Top);
        }
    }
}