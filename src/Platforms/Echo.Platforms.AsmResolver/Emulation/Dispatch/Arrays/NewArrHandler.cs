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
            
            var elementType = (ITypeDefOrRef) instruction.Operand!;
            var elementCount = stack.Pop();
            
            try
            {
                var address = context.Machine.ValueFactory.RentNativeInteger(false); 
                
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
                context.Machine.ValueFactory.BitVectorPool.Return(elementCount.Contents);
            }
            
            return CilDispatchResult.Success();
        }
    }
}