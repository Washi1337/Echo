using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>unbox.any</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Unbox_Any)]
    public class UnboxAnyHandler : UnboxHandlerBase
    {
        /// <inheritdoc />
        protected override CilDispatchResult HandleNull(CilExecutionContext context, TypeSignature targetType)
        {
            // TODO: Handle Nullable<T>

            // Null references cannot be unboxed if the type is a structure.
            if (targetType.IsValueType)
                return CilDispatchResult.NullReference(context);
            
            // If it is a reference type, unbox.any == castclass which allows for null to be casted.
            return base.HandleNull(context, targetType);
        }

        /// <inheritdoc />
        protected override StackSlot GetReturnValue(
            CilExecutionContext context, 
            ObjectHandle handle, 
            TypeSignature targetType)
        {
            // TODO: Handle Nullable<T>

            if (targetType.IsValueType)
            {
                // If it is a value type, read the structure in the boxed object.
                var value = context.Machine.ValueFactory.RentValue(targetType, false);
                handle.ReadObjectData(value);
                return new StackSlot(value, StackSlotTypeHint.Structure);
            }
            else
            {
                // If it is a reference type, unbox.any == castclass, i.e., just push the object pointer.
                var value = context.Machine.ValueFactory.RentNativeInteger(handle.Address);
                return new StackSlot(value, StackSlotTypeHint.Integer);
            }
        }
    }
}