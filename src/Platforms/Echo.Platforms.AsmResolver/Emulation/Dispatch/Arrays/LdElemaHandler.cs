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
            var arrayIndex = stack.Pop();
            var arrayAddress = stack.Pop();
            var result = factory.RentNativeInteger(false);
            var arrayLength = factory.RentNativeInteger(false);

            try
            {
                // Concretize pushed address.
                var arrayAddressSpan = arrayAddress.Contents.AsSpan();
                long? resolvedAddress = arrayAddressSpan.IsFullyKnown
                    ? arrayAddressSpan.ReadNativeInteger(context.Machine.Is32Bit)
                    : context.Machine.UnknownResolver.ResolveSourcePointer(context, instruction, arrayAddress);

                switch (resolvedAddress)
                {
                    case null:
                        // If address is unknown even after resolution, assume it points to "some element" successfully.
                        break;

                    case 0:
                        // A null reference was passed.
                        return CilDispatchResult.NullReference(context);

                    case { } actualAddress:
                        // A non-null reference was passed.
                        context.Machine.Memory.Read(actualAddress + factory.ArrayLengthOffset, arrayLength);
                        
                        // Concretize pushed index.
                        var arrayIndexSpan = arrayIndex.Contents.AsSpan();
                        long? resolvedIndex = arrayIndexSpan.IsFullyKnown
                            ? arrayIndexSpan.ReadNativeInteger(context.Machine.Is32Bit)
                            : context.Machine.UnknownResolver.ResolveArrayIndex(context, instruction, actualAddress, arrayIndex);

                        // Leave result unknown if index is not fully known.
                        if (resolvedIndex.HasValue && arrayLength.AsSpan().IsFullyKnown)
                        {
                            // Bounds check.
                            if (resolvedIndex >= arrayLength.AsSpan().ReadNativeInteger(context.Machine.Is32Bit))
                                return CilDispatchResult.IndexOutOfRange(context);
                            
                            long elementAddress = actualAddress
                                .ToObjectHandle(context.Machine)
                                .GetArrayElementAddress(elementType, resolvedIndex.Value);

                            result.AsSpan().WriteNativeInteger(elementAddress, context.Machine.Is32Bit);
                        }

                        break;
                }
                
                // Push.
                stack.Push(new StackSlot(result, StackSlotTypeHint.Integer));
                return CilDispatchResult.Success();
            }
            finally
            {
                factory.BitVectorPool.Return(arrayLength);
                factory.BitVectorPool.Return(arrayAddress.Contents);
                factory.BitVectorPool.Return(arrayIndex.Contents);
            }
        }
    }
}