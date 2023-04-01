using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>initblk</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Initblk)]
    public class InitBlkHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            
            var size = stack.Pop();
            var value = stack.Pop(context.Machine.ContextModule.CorLibTypeFactory.Byte); 
            var address = stack.Pop();
            
            try
            {
                // Object/structure was pushed by reference onto the stack. Concretize address and size.
                var addressSpan = address.Contents.AsSpan();
                long? resolvedAddress = addressSpan.IsFullyKnown
                    ? addressSpan.ReadNativeInteger(context.Machine.Is32Bit)
                    : context.Machine.UnknownResolver.ResolveDestinationPointer(context, instruction, address);

                var sizeSpan = size.Contents.AsSpan();
                uint resolvedSize = sizeSpan.IsFullyKnown
                    ? sizeSpan.U32
                    : context.Machine.UnknownResolver.ResolveBlockSize(context, instruction, size);

                switch (resolvedAddress)
                {
                    case null:
                        // If address is unknown even after resolution, assume it writes to "somewhere" successfully.
                        return CilDispatchResult.Success();

                    case 0:
                        // A null reference was passed.
                        return CilDispatchResult.NullReference(context);

                    case { } actualAddress:
                        // A non-null reference was passed.

                        // Allocate a temporary buffer to write into memory.
                        var buffer = new BitVector((int) (resolvedSize * 8), false);
                        buffer.AsSpan().Fill(value.Bits[0], value.KnownMask[0]);

                        // Write it.
                        context.Machine.Memory.Write(actualAddress, buffer);
                        break;
                }
            }
            finally
            {
                factory.BitVectorPool.Return(address.Contents);
                factory.BitVectorPool.Return(value);
                factory.BitVectorPool.Return(size.Contents);
            }
            
            return CilDispatchResult.Success();
        }
    }
}