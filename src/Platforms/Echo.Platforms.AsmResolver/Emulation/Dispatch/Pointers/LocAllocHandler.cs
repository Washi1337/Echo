using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>localloc</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Localloc)]
    public class LocAllocHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            var size = stack.Pop();

            try
            {
                var sizeSpan = size.Contents.AsSpan();
                uint resolvedSize = sizeSpan.IsFullyKnown
                    ? sizeSpan.U32
                    : context.Machine.UnknownResolver.ResolveBlockSize(context, instruction, size);
                
                var address = factory.RentNativeInteger(context.CurrentFrame.Allocate((int) resolvedSize));
                stack.Push(new StackSlot(address, StackSlotTypeHint.Integer));

                return CilDispatchResult.Success();
            }
            catch (StackOverflowException)
            {
                return CilDispatchResult.StackOverflow(context);
            }
            finally
            {
                factory.BitVectorPool.Return(size.Contents);
            }
        }
    }
}