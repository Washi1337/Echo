using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldelema</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldelema)]
    public class LdElemaHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            // Determine parameters.
            var elementType = ((ITypeDefOrRef) instruction.Operand!).ToTypeSignature();
            var arrayIndex = stack.Pop().Contents;
            var arrayAddress = stack.Pop().Contents;
            var result = factory.RentNativeInteger(false);

            try
            {
                // Read element if fully known address and index, else leave result unknown.
                var arrayAddressSpan = arrayAddress.AsSpan();
                var arrayIndexSpan = arrayIndex.AsSpan();

                switch (arrayAddressSpan)
                {
                    case { IsFullyKnown: false }:
                        break;

                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);

                    default:
                        long actualArrayAddress = arrayAddressSpan.ReadNativeInteger(context.Machine.Is32Bit);
                        var arraySpan = context.Machine.Heap.GetObjectSpan(actualArrayAddress);
                        long length = arraySpan.SliceArrayLength(factory).ReadNativeInteger(context.Machine.Is32Bit);

                        // Leave result unknown if index is not fully known.
                        if (arrayIndexSpan.IsFullyKnown)
                        {
                            int index = arrayIndexSpan.I32;
                            if (index >= length)
                                return CilDispatchResult.IndexOutOfRange(context);
                            
                            uint elementSize = factory.GetTypeValueMemoryLayout(elementType).Size;
                            result.AsSpan().WriteNativeInteger(
                                actualArrayAddress + factory.ArrayHeaderSize + elementSize * index, 
                                context.Machine.Is32Bit);
                        }

                        break;
                }
                
                // Push.
                stack.Push(new StackSlot(result, StackSlotTypeHint.Integer));
                return CilDispatchResult.Success();
            }
            finally
            {
                factory.BitVectorPool.Return(arrayAddress);
                factory.BitVectorPool.Return(arrayIndex);
            }
        }
    }
}