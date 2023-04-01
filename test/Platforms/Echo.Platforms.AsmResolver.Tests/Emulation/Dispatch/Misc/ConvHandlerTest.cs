using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
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
        [InlineData(0x7f, CilCode.Conv_I1, 0x7f)]
        [InlineData(0x7f, CilCode.Conv_Ovf_I1, 0x7f)]
        [InlineData(0x80, CilCode.Conv_I1, -0x80)]
        [InlineData(0x80, CilCode.Conv_Ovf_I1, null)]
        [InlineData(0x80, CilCode.Conv_I2, 0x80)]
        [InlineData(0x7fff, CilCode.Conv_I2, 0x7fff)]
        [InlineData(0x8000, CilCode.Conv_I2, -0x8000)]
        [InlineData(0x8000, CilCode.Conv_Ovf_I2, null)]
        [InlineData(0x7fffffff, CilCode.Conv_I4, 0x7fffffff)]
        [InlineData(0x80000000, CilCode.Conv_I4, -0x80000000)]
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
        [InlineData(1.0, CilCode.Conv_I4, 1)]
        [InlineData(-1.0, CilCode.Conv_I4, -1)]
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