using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class LdIndHandlerTest : CilOpCodeHandlerTestBase
    {
        public LdIndHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void ReadFromNullShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldind_I4));

            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.ToObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
        }

        [Fact]
        public void ReadFromUnknownShouldPushUnknown()
        {
            Context.Machine.UnknownResolver = EmptyUnknownResolver.Instance;
            
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(Context.Machine.ValueFactory.CreateNativeInteger(false), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldind_I4));

            Assert.True(result.IsSuccess);
            Assert.Single(stack);
            Assert.False(stack.Peek().Contents.AsSpan().IsFullyKnown);
        }

        [Theory]
        [InlineData(CilCode.Ldind_I1, 0x00)]
        [InlineData(CilCode.Ldind_I2, 0x0100)]
        [InlineData(CilCode.Ldind_I4, 0x03020100)]
        [InlineData(CilCode.Ldind_U1, 0x00)]
        [InlineData(CilCode.Ldind_U2, 0x0100)]
        [InlineData(CilCode.Ldind_U4, 0x03020100)]
        public void ReadSmallIntFromPointer(CilCode code, int expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            const long address = 0x0600_0000;
            Context.Machine.Memory.Map(address, new BasicMemorySpace(new byte[]
            {
                0x00, 0x01, 0x02, 0x03
            }));
            stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));
            
            Assert.True(result.IsSuccess);

            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            Assert.Equal(expected, slot.Contents.AsSpan().I32);
        }

        [Fact]
        public void ReadLargeIntFromPointer()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            const long address = 0x0600_0000;
            Context.Machine.Memory.Map(address, new BasicMemorySpace(new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            }));
            stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldind_I8));
            
            Assert.True(result.IsSuccess);

            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            Assert.Equal(0x0706050403020100, slot.Contents.AsSpan().I64);
        }

        [Fact]
        public void ReadExplicitIntFromPointer()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            const long address = 0x0600_0000;
            Context.Machine.Memory.Map(address, new BasicMemorySpace(new byte[]
            {
                0x00, 0x01, 0x02, 0x03
            }));
            stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(
                CilOpCodes.Ldobj, 
                Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type));
            
            Assert.True(result.IsSuccess);

            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            Assert.Equal(0x03020100, slot.Contents.AsSpan().I32);
        }
    }
}