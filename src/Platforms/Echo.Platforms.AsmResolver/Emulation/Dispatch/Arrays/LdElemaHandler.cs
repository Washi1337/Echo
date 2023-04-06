using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
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
            var genericContext = GenericContext.FromMethod(context.CurrentFrame.Method);

            // Determine parameters.
            var elementType = ((ITypeDefOrRef) instruction.Operand!).ToTypeSignature().InstantiateGenericTypes(genericContext);
            var arrayIndex = stack.Pop();
            var arrayAddress = stack.Pop();
            var result = factory.RentNativeInteger(false);
            var arrayLength = factory.BitVectorPool.Rent(32, false);

            try
            {
                // Concretize pushed address.
                long? resolvedAddress = arrayAddress.Contents.IsFullyKnown
                    ? arrayAddress.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
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
                        var handle = actualAddress.AsObjectHandle(context.Machine);
                        
                        // Concretize pushed index.
                        long? resolvedIndex = arrayIndex.Contents.IsFullyKnown
                            ? arrayIndex.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
                            : context.Machine.UnknownResolver.ResolveArrayIndex(context, instruction, actualAddress, arrayIndex);

                        // Leave result unknown if index is not fully known.
                        handle.ReadArrayLength(arrayLength);
                        if (resolvedIndex.HasValue && arrayLength.IsFullyKnown)
                        {
                            // Bounds check.
                            if (resolvedIndex >= arrayLength.AsSpan().ReadNativeInteger(context.Machine.Is32Bit))
                                return CilDispatchResult.IndexOutOfRange(context);
                            
                            long elementAddress = handle.GetArrayElementAddress(elementType, resolvedIndex.Value);
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