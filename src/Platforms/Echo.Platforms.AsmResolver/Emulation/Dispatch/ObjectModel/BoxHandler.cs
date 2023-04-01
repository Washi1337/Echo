using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>box</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Box)]
    public class BoxHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var type = (ITypeDefOrRef) instruction.Operand!;
            
            // For reference types, a box instruction is equivalent to a NOP.
            if (!type.IsValueType)
                return CilDispatchResult.Success();

            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            // Pop structure / value type.
            var value = stack.Pop(type.ToTypeSignature(true));

            try
            {
                // Allocate box object.
                long address = context.Machine.Heap.AllocateObject(type, false);
                
                // Copy the value into the object data.
                context.Machine.Heap
                    .GetObjectSpan(address)
                    .SliceObjectData(factory)
                    .Write(value);

                // Push box object address.
                var result = factory.RentNativeInteger(address);
                stack.Push(new StackSlot(result, StackSlotTypeHint.Integer));
            }
            finally
            {
                factory.BitVectorPool.Return(value);
            }

            return CilDispatchResult.Success();
        }
    }
}