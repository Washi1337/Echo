using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>newarr</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Newarr)]
    public class NewArrHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            
            var elementType = (ITypeDefOrRef) instruction.Operand!;
            var elementCount = stack.Pop();

            try
            {
                var address = factory.RentNativeInteger(false); 
                
                // Only actually allocate the array if the element count is known, otherwise, we can leave the 
                // pointer unknown as we don't know the total size of the object.
                var countSpan = elementCount.Contents.AsSpan();
                if (countSpan.IsFullyKnown)
                {
                    long actual = context.Machine.Heap.AllocateSzArray(elementType.ToTypeSignature(), countSpan.I32, true);
                    address.AsSpan().WriteNativeInteger(actual, context.Machine.Is32Bit);
                }

                stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));
            }
            finally
            {
                factory.BitVectorPool.Return(elementCount.Contents);
            }
            
            return CilDispatchResult.Success();
        }
    }
}