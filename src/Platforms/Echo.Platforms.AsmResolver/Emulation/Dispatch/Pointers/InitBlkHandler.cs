using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Core;

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
                var addressSpan = address.Contents.AsSpan();
                switch (addressSpan)
                {
                    case { IsFullyKnown: false }:
                        // TODO: make configurable
                        throw new CilEmulatorException("Attempted to initialize memory at an unknown pointer.");
                    
                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);
                    
                    default:
                        var sizeSpan = size.Contents.AsSpan();
                        if (!sizeSpan.IsFullyKnown)
                            throw new CilEmulatorException("Attempted to initialize memory with an unknown size.");

                        // Allocate a temporary buffer to write into memory.
                        var buffer = new BitVector(sizeSpan.I32 * 8, false);
                        buffer.AsSpan().Fill(value.Bits[0], value.KnownMask[0]);

                        // Write it.
                        context.Machine.Memory.Write(
                            addressSpan.ReadNativeInteger(context.Machine.Is32Bit),
                            buffer);
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