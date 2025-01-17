using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Misc
{
    public class ConvHandlerTest : CilOpCodeHandlerTestBase
    {
        public ConvHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(0x7f, CilCode.Conv_I1, 0x7fL)]
        [InlineData(0x7f, CilCode.Conv_Ovf_I1, 0x7fL)]
        [InlineData(0x80, CilCode.Conv_I1, -0x80L)]
        [InlineData(0x80, CilCode.Conv_Ovf_I1, null)]
        [InlineData(0x80, CilCode.Conv_I2, 0x80L)]
        [InlineData(0x7fff, CilCode.Conv_I2, 0x7fffL)]
        [InlineData(0x8000, CilCode.Conv_I2, -0x8000L)]
        [InlineData(0x8000, CilCode.Conv_Ovf_I2, null)]
        [InlineData(0x7fffffff, CilCode.Conv_I4, 0x7fffffffL)]
        [InlineData(0x80000000, CilCode.Conv_I4, -0x80000000L)]
        [InlineData(0x80000000, CilCode.Conv_Ovf_I4, null)]
        public void ConvIToI(long value, CilCode code, long? expectedValue)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Int64);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));
            
            Assert.Equal(expectedValue.HasValue, result.IsSuccess);
            if (result.IsSuccess)
                Assert.Equal(expectedValue!.Value, stack.Peek().Contents.Resize(64, true).AsSpan().I64);
        }

        [Theory]
        [InlineData(1.0, CilCode.Conv_I4, 1L)]
        [InlineData(-1.0, CilCode.Conv_I4, -1L)]
        public void ConvFToI(double value, CilCode code, long? expectedValue)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Double);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));
            
            Assert.Equal(expectedValue.HasValue, result.IsSuccess);
            if (result.IsSuccess)
                Assert.Equal(expectedValue!.Value, stack.Peek().Contents.Resize(64, true).AsSpan().I64);
        }
    }
}