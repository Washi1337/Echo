using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class CpObjHandlerTest : CilOpCodeHandlerTestBase
    {
        public CpObjHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void CopyFromNullShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var type = Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type;

            // Map some destination.
            long destinationAddress = 0x0600_0000;
            Context.Machine.Memory.Map(destinationAddress, new BasicMemorySpace(10, false));

            // Push null, a value, and a size.
            stack.Push(new StackSlot(destinationAddress, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(Context.Machine.ValueFactory.CreateNull(), StackSlotTypeHint.Integer));

            // Dispatch.
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Cpobj, type));

            // Verify
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionObject.GetMethodTable().Type;
            Assert.Equal("System.NullReferenceException", exceptionType.FullName);
        }

        [Fact]
        public void CopyToNullShouldThrow()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            var type = Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type;

            // Map some destination.
            long sourceAddress = 0x0600_0000;
            Context.Machine.Memory.Map(sourceAddress, new BasicMemorySpace(new byte[] {1, 2, 3, 4}));

            // Push null, a value, and a size.
            stack.Push(new StackSlot(Context.Machine.ValueFactory.CreateNull(), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(sourceAddress, StackSlotTypeHint.Integer));

            // Dispatch.
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Cpobj, type));

            // Verify
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionObject.GetMethodTable().Type;
            Assert.Equal("System.NullReferenceException", exceptionType.FullName);
        }

        [Fact]
        public void CopyValidAddresses()
        {
            byte[] data = {
                0x00, 0x01, 0x02, 0x03
            };

            var stack = Context.CurrentFrame.EvaluationStack;
            var type = Context.Machine.ContextModule.CorLibTypeFactory.Int32.Type;

            // Map some source.
            long sourceAddress = 0x0600_0000;
            long destinationAddress = 0x0700_0000;
            Context.Machine.Memory.Map(sourceAddress, new BasicMemorySpace(data));
            Context.Machine.Memory.Map(destinationAddress, new BasicMemorySpace(data.Length, false));
        
            // Push null, a value, and a size.
            stack.Push(new StackSlot(destinationAddress, StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(sourceAddress, StackSlotTypeHint.Integer));

            // Dispatch.
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Cpobj, type));

            // Verify
            Assert.True(result.IsSuccess);

            var buffer = new BitVector(data.Length * 8, false);
            Context.Machine.Memory.Read(destinationAddress, buffer);
            Assert.Equal(data, buffer.Bits);
        }

    }
}