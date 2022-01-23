using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Constants
{
    public class LdcHandlerTest : CilOpCodeHandlerTestBase
    {
        public LdcHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(CilCode.Ldc_I4, 13371337)]
        [InlineData(CilCode.Ldc_I4_S, (sbyte) 12)]
        [InlineData(CilCode.Ldc_I4_0, null)]
        [InlineData(CilCode.Ldc_I4_1, null)]
        [InlineData(CilCode.Ldc_I4_2, null)]
        [InlineData(CilCode.Ldc_I4_3, null)]
        [InlineData(CilCode.Ldc_I4_4, null)]
        [InlineData(CilCode.Ldc_I4_5, null)]
        [InlineData(CilCode.Ldc_I4_6, null)]
        [InlineData(CilCode.Ldc_I4_7, null)]
        [InlineData(CilCode.Ldc_I4_8, null)]
        [InlineData(CilCode.Ldc_I4_M1, null)]
        public void PushI4(CilCode code, object operand)
        {
            var instruction = new CilInstruction(code.ToOpCode(), operand);

            var result = Dispatcher.Dispatch(Context, instruction);
            Assert.True(result.IsSuccess);
            var span = Context.CurrentFrame.EvaluationStack.Peek().Contents.AsSpan();
            Assert.True(span.IsFullyKnown);
            Assert.Equal(instruction.GetLdcI4Constant(), span.I32);
        }

        [Fact]
        public void PushI8()
        {
            var instruction = new CilInstruction(CilOpCodes.Ldc_I8, 0x1337133713371337L);

            var result = Dispatcher.Dispatch(Context, instruction);
            Assert.True(result.IsSuccess);
            var span = Context.CurrentFrame.EvaluationStack.Peek().Contents.AsSpan();
            Assert.True(span.IsFullyKnown);
            Assert.Equal(0x1337133713371337L, span.I64);
        }

        [Fact]
        public void PushR4()
        {
            var instruction = new CilInstruction(CilOpCodes.Ldc_R4, 1.23456789f);

            var result = Dispatcher.Dispatch(Context, instruction);
            Assert.True(result.IsSuccess);
            var span = Context.CurrentFrame.EvaluationStack.Peek().Contents.AsSpan();
            Assert.True(span.IsFullyKnown);
            Assert.Equal(1.23456789f, span.F64);
        }

        [Fact]
        public void PushR8()
        {
            var instruction = new CilInstruction(CilOpCodes.Ldc_R8, 1.23456789D);

            var result = Dispatcher.Dispatch(Context, instruction);
            Assert.True(result.IsSuccess);
            var span = Context.CurrentFrame.EvaluationStack.Peek().Contents.AsSpan();
            Assert.True(span.IsFullyKnown);
            Assert.Equal(1.23456789D, span.F64);
        }
    }
}