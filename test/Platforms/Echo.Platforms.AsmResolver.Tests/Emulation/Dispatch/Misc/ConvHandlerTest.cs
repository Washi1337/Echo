using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Stack;
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
        [InlineData(-1, CilCode.Conv_I1, -1)]
        [InlineData(0x1ff, CilCode.Conv_I1, -1)]
        [InlineData(0x80, CilCode.Conv_I2, 0x80)]
        [InlineData(0x7fff, CilCode.Conv_I2, 0x7fff)]
        [InlineData(0x8000, CilCode.Conv_I2, -0x8000)]
        [InlineData(0x1ffff, CilCode.Conv_I2, -1)]
        [InlineData(0x7fffffff, CilCode.Conv_I4, 0x7fffffff)]
        [InlineData(0x80000000L, CilCode.Conv_I4, -0x80000000)]
        [InlineData(0x1ffffffffL, CilCode.Conv_I4, -1)]
        [InlineData(-1, CilCode.Conv_I8, -1)]
        [InlineData(0, CilCode.Conv_I8, 0)]
        [InlineData(0x7fffffffL, CilCode.Conv_I8, 0x7fffffff)]
        public void ConvIToI(long value, CilCode code, long expectedValue)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Int64);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.True(result.IsSuccess);
            var span = stack.Peek().Contents.AsSpan();
            if (code.ToOpCode().StackBehaviourPush == CilStackBehaviour.PushI8)
                Assert.Equal(expectedValue, span.I64);
            else
                Assert.Equal((int) expectedValue, span.I32);
        }

        [Theory]
        [InlineData(0x42, CilCode.Conv_U1, 0x42)]
        [InlineData(0xff, CilCode.Conv_U1, 0xff)]
        [InlineData(0x1ff, CilCode.Conv_U1, 0xff)]
        [InlineData(-1, CilCode.Conv_U1, 0xff)]
        [InlineData(0x1234, CilCode.Conv_U2, 0x1234)]
        [InlineData(0xffff, CilCode.Conv_U2, 0xffff)]
        [InlineData(0x1ffff, CilCode.Conv_U2, 0xffff)]
        [InlineData(-1, CilCode.Conv_U2, 0xffff)]
        [InlineData(0x12345678, CilCode.Conv_U4, 0x12345678)]
        [InlineData(0xffffffffL, CilCode.Conv_U4, unchecked((int) 0xffffffff))]
        [InlineData(0x1ffffffffL, CilCode.Conv_U4, unchecked((int) 0xffffffff))]
        [InlineData(-1, CilCode.Conv_U4, unchecked((int) 0xffffffff))]
        [InlineData(42, CilCode.Conv_U8, 42)]
        [InlineData(0, CilCode.Conv_U8, 0)]
        [InlineData(-1, CilCode.Conv_U8, -1)]
        public void ConvIToU(long value, CilCode code, long expectedValue)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Int64);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.True(result.IsSuccess);
            var span = stack.Peek().Contents.AsSpan();
            if (code.ToOpCode().StackBehaviourPush == CilStackBehaviour.PushI8)
                Assert.Equal(expectedValue, span.I64);
            else
                Assert.Equal((int) expectedValue, span.I32);
        }

        [Theory]
        [InlineData(42, CilCode.Conv_R4, 42.0f)]
        [InlineData(-1, CilCode.Conv_R4, -1.0f)]
        [InlineData(0, CilCode.Conv_R4, 0.0f)]
        [InlineData(0x1_0000_0000L, CilCode.Conv_R4, 4294967296.0f)]
        [InlineData((long) int.MaxValue, CilCode.Conv_R4, (float) int.MaxValue)]
        [InlineData((long) int.MinValue, CilCode.Conv_R4, (float) int.MinValue)]
        public void ConvIToR4(long value, CilCode code, float expectedValue)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Int64);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, stack.Peek().Contents.AsSpan().F32);
        }

        [Theory]
        [InlineData(42, CilCode.Conv_R8, 42.0d)]
        [InlineData(-1, CilCode.Conv_R8, -1.0d)]
        [InlineData(0, CilCode.Conv_R8, 0.0d)]
        [InlineData(0x1_0000_0000L, CilCode.Conv_R8, 4294967296.0d)]
        [InlineData((long) int.MaxValue, CilCode.Conv_R8, (double) int.MaxValue)]
        [InlineData((long) int.MinValue, CilCode.Conv_R8, (double) int.MinValue)]
        public void ConvIToR8(long value, CilCode code, double expectedValue)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Int64);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, stack.Peek().Contents.AsSpan().F64);
        }

        [Fact]
        public void ConvUnknownFloatToR4ShouldHaveFloatTypeHint()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(new BitVector(64, false), StackSlotTypeHint.Float));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Conv_R4));

            Assert.True(result.IsSuccess);
            Assert.Equal(StackSlotTypeHint.Float, stack.Peek().TypeHint);
        }

        [Fact]
        public void ConvUnknownFloatToI4ShouldHaveIntegerTypeHint()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(new BitVector(64, false), StackSlotTypeHint.Float));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Conv_I4));

            Assert.True(result.IsSuccess);
            Assert.Equal(StackSlotTypeHint.Integer, stack.Peek().TypeHint);
        }

        [Theory]
        [InlineData(1.0d, CilCode.Conv_I1, 1)]
        [InlineData(-1.0d, CilCode.Conv_I1, -1)]
        [InlineData(100.9d, CilCode.Conv_I1, 100)]
        [InlineData(1.0d, CilCode.Conv_I2, 1)]
        [InlineData(-1.0d, CilCode.Conv_I2, -1)]
        [InlineData(1000.9d, CilCode.Conv_I2, 1000)]
        [InlineData(1.0d, CilCode.Conv_I4, 1)]
        [InlineData(-1.0d, CilCode.Conv_I4, -1)]
        [InlineData(3.7d, CilCode.Conv_I4, 3)]
        [InlineData(-3.7d, CilCode.Conv_I4, -3)]
        [InlineData(0.0d, CilCode.Conv_I4, 0)]
        [InlineData(0.0d, CilCode.Conv_I8, 0)]
        [InlineData(42.0d, CilCode.Conv_I8, 42)]
        [InlineData(-1.0d, CilCode.Conv_I8, -1)]
        [InlineData(99999.9d, CilCode.Conv_I8, 99999)]
        public void ConvRToI(double value, CilCode code, long expectedValue)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Double);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.True(result.IsSuccess);
            var span = stack.Peek().Contents.AsSpan();
            if (code.ToOpCode().StackBehaviourPush == CilStackBehaviour.PushI8)
                Assert.Equal(expectedValue, span.I64);
            else
                Assert.Equal((int) expectedValue, span.I32);
        }

        [Theory]
        [InlineData(42.0d, CilCode.Conv_U1, 42)]
        [InlineData(0.0d, CilCode.Conv_U1, 0)]
        [InlineData(200.9d, CilCode.Conv_U1, 200)]
        [InlineData(42.0d, CilCode.Conv_U2, 42)]
        [InlineData(0.0d, CilCode.Conv_U2, 0)]
        [InlineData(50000.9d, CilCode.Conv_U2, 50000)]
        [InlineData(42.0d, CilCode.Conv_U4, 42)]
        [InlineData(3.7d, CilCode.Conv_U4, 3)]
        [InlineData(0.0d, CilCode.Conv_U4, 0)]
        [InlineData(42.0d, CilCode.Conv_U8, 42)]
        [InlineData(0.0d, CilCode.Conv_U8, 0)]
        [InlineData(99999.9d, CilCode.Conv_U8, 99999)]
        public void ConvRToU(double value, CilCode code, long expectedValue)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Double);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.True(result.IsSuccess);
            var span = stack.Peek().Contents.AsSpan();
            if (code.ToOpCode().StackBehaviourPush == CilStackBehaviour.PushI8)
                Assert.Equal(expectedValue, span.I64);
            else
                Assert.Equal((int) expectedValue, span.I32);
        }

        [Theory]
        [InlineData(0x80, CilCode.Conv_Ovf_I1)]
        [InlineData(-0x81, CilCode.Conv_Ovf_I1)]
        [InlineData(0x8000, CilCode.Conv_Ovf_I2)]
        [InlineData(-0x8001, CilCode.Conv_Ovf_I2)]
        [InlineData(0x80000000L, CilCode.Conv_Ovf_I4)]
        [InlineData(0x100, CilCode.Conv_Ovf_U1)]
        [InlineData(-1, CilCode.Conv_Ovf_U1)]
        [InlineData(0x10000, CilCode.Conv_Ovf_U2)]
        [InlineData(-1, CilCode.Conv_Ovf_U2)]
        [InlineData(0x100000000L, CilCode.Conv_Ovf_U4)]
        [InlineData(-1, CilCode.Conv_Ovf_U4)]
        public void ConvIOvfShouldOverflow(long value, CilCode code)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Int64);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.False(result.IsSuccess);
            Assert.False(result.ExceptionObject.IsNull);
            Assert.Equal(result.ExceptionObject.GetObjectType(), Context.Machine.ValueFactory.OverflowExceptionType);
        }

        [Theory]
        [InlineData(-1.0d, CilCode.Conv_Ovf_U1)]
        [InlineData(256.0d, CilCode.Conv_Ovf_U1)]
        [InlineData(-1.0d, CilCode.Conv_Ovf_U2)]
        [InlineData(65536.0d, CilCode.Conv_Ovf_U2)]
        [InlineData(-1.0d, CilCode.Conv_Ovf_U4)]
        [InlineData(-1.0d, CilCode.Conv_Ovf_U8)]
        [InlineData(128.0d, CilCode.Conv_Ovf_I1)]
        [InlineData(-129.0d, CilCode.Conv_Ovf_I1)]
        [InlineData(32768.0d, CilCode.Conv_Ovf_I2)]
        [InlineData(-32769.0d, CilCode.Conv_Ovf_I2)]
        public void ConvROvfShouldOverflow(double value, CilCode code)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new BitVector(value), Context.Machine.ContextModule.CorLibTypeFactory.Double);

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.False(result.IsSuccess);
            Assert.False(result.ExceptionObject.IsNull);
            Assert.Equal(result.ExceptionObject.GetObjectType(), Context.Machine.ValueFactory.OverflowExceptionType);
        }
    }
}