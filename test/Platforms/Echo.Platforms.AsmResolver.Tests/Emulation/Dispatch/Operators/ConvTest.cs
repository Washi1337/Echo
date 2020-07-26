using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class ConvTest : DispatcherTestBase
    {
        public ConvTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Theory]
        [InlineData(CilCode.Conv_I1, 0, 0)]
        [InlineData(CilCode.Conv_Ovf_I1, -1, -1)]
        [InlineData(CilCode.Conv_Ovf_I1_Un, -1, null)]
        [InlineData(CilCode.Conv_Ovf_I1_Un, 0x7F, 0x7F)]
        [InlineData(CilCode.Conv_U1, 0, 0)]
        [InlineData(CilCode.Conv_Ovf_U1, -1, null)]
        [InlineData(CilCode.Conv_Ovf_U1_Un, -1, null)]
        [InlineData(CilCode.Conv_Ovf_U1_Un, 0x7F, 0x7F)]
        [InlineData(CilCode.Conv_I2, 0, 0)]
        [InlineData(CilCode.Conv_Ovf_I2, -1, -1)]
        [InlineData(CilCode.Conv_Ovf_I2_Un, -1, null)]
        [InlineData(CilCode.Conv_Ovf_I2_Un, 0x7FFF, 0x7FFF)]
        [InlineData(CilCode.Conv_U2, 0, 0)]
        [InlineData(CilCode.Conv_Ovf_U2, -1, null)]
        [InlineData(CilCode.Conv_Ovf_U2_Un, -1, null)]
        [InlineData(CilCode.Conv_Ovf_U2_Un, 0x7FFF, 0x7FFF)]
        public void ConvertI4OnStack(CilCode code, int value, int? expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(value));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));

            Assert.Equal(expected.HasValue, result.IsSuccess);

            if (expected.HasValue)
                Assert.Equal(new I4Value(expected.Value), stack.Top);
            else
                Assert.IsAssignableFrom<OverflowException>(result.Exception);
        }
        
        [Theory]
        [InlineData(CilCode.Conv_I8, 0x7fffffff, 0x7fffffffL)]
        [InlineData(CilCode.Conv_Ovf_I8, -1, -1L)]
        [InlineData(CilCode.Conv_Ovf_I8_Un, -1, 0xffffffffL)]
        [InlineData(CilCode.Conv_U8, 0x7fffffff, 0x7fffffffL)]
        [InlineData(CilCode.Conv_Ovf_U8, -1, null)]
        [InlineData(CilCode.Conv_Ovf_U8_Un, -1, 0xffffffffL)]
        public void ConvertI4OnStackToInt64(CilCode code, int value, long? expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(value));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));

            Assert.Equal(expected.HasValue, result.IsSuccess);

            if (expected.HasValue)
                Assert.Equal(new I8Value(expected.Value), stack.Top);
            else
                Assert.IsAssignableFrom<OverflowException>(result.Exception);
        }
        
    }
}