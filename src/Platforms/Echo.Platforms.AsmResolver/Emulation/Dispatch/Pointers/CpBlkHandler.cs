using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>cpblk</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Cpblk)]
    public class CpBlkHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            var size = stack.Pop();
            var source = stack.Pop();
            var destination = stack.Pop();

            try
            {
                var sourceSpan = source.Contents.AsSpan();
                var destinationSpan = destination.Contents.AsSpan();
                var sizeSpan = size.Contents.AsSpan();

                // TODO: make configurable.
                if (!sourceSpan.IsFullyKnown)
                    throw new CilEmulatorException("Attempted to read memory at an unknown pointer.");
                if (!destinationSpan.IsFullyKnown)
                    throw new CilEmulatorException("Attempted to write memory at an unknown pointer.");
                if (!sizeSpan.IsFullyKnown)
                    throw new CilEmulatorException("Attempted to copy an unknown amount of bytes.");
                
                // Check for null addresses.
                if (destinationSpan.IsZero || sourceSpan.IsZero)
                    return CilDispatchResult.NullReference(context);

                // Get addresses.
                long sourceAddress = sourceSpan.ReadNativeInteger(context.Machine.Is32Bit);
                long destinationAddress = destinationSpan.ReadNativeInteger(context.Machine.Is32Bit);

                // Perform the copy.
                var buffer = new BitVector(size.Contents.AsSpan().I32 * 8, false);
                context.Machine.Memory.Read(sourceAddress, buffer);
                context.Machine.Memory.Write(destinationAddress, buffer);
            }
            finally
            {
                factory.BitVectorPool.Return(destination.Contents);
                factory.BitVectorPool.Return(source.Contents);
                factory.BitVectorPool.Return(size.Contents);
            }
            
            return CilDispatchResult.Success();
        }
    }
}