using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class StIndHandlerTest : CilOpCodeHandlerTestBase
    {
        public StIndHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void WriteToNullShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(Context.Machine.ValueFactory.CreateNull(), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stind_I4));

            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionObject.GetMethodTable().Type;
            Assert.Equal("System.NullReferenceException", exceptionType.FullName);
        }

        [Theory]
        [InlineData(CilCode.Stind_I1, 0x01, new byte[] { 0x01, 0x00, 0x00, 0x00 })]
        [InlineData(CilCode.Stind_I2, 0x0201, new byte[] { 0x01, 0x02, 0x00, 0x00 })]
        [InlineData(CilCode.Stind_I4, 0x04030201, new byte[] { 0x01, 0x02, 0x03, 0x04 })]
        public void WriteSmallIntToPointer(CilCode code, int value, byte[] expected)
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            const long address = 0x0600_0000;
            Context.Machine.Memory.Map(address, new BasicMemorySpace(new byte[]
            {
                0x00, 0x00, 0x00, 0x00
            }));
            stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(value, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(code.ToOpCode()));

            Assert.True(result.IsSuccess);
            Assert.Empty(stack);

            byte[] data = new byte[4];
            Context.Machine.Memory.Read(address, new BitVector(data));
            Assert.Equal(expected, data);
        }

        [Fact]
        public void WriteLargeIntToPointer()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            const long address = 0x0600_0000;
            Context.Machine.Memory.Map(address, new BasicMemorySpace(new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            }));
            stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(0x0807060504030201, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stind_I));

            Assert.True(result.IsSuccess);
            Assert.Empty(stack);

            byte[] data = new byte[8];
            Context.Machine.Memory.Read(address, new BitVector(data));
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }, data);
        }

        [Fact]
        public void WriteExplicitIntToPointer()
        {
            var stack = Context.CurrentFrame.EvaluationStack;

            const long address = 0x0600_0000;
            Context.Machine.Memory.Map(address, new BasicMemorySpace(new byte[]
            {
                0x00, 0x00, 0x00, 0x00
            }));
            stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(0x04030201, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(
                CilOpCodes.Stobj, 
                Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type));

            Assert.True(result.IsSuccess);
            Assert.Empty(stack);

            byte[] data = new byte[4];
            Context.Machine.Memory.Read(address, new BitVector(data));
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, data);
        }

    }
}