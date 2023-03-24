using AsmResolver.DotNet.Signatures.Types;
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
                return base.HandleNull(context, targetType);
            
            // If it is a reference type, unbox.any == castclass which allows for null to be casted.
            var value = context.Machine.ValueFactory.RentNativeInteger(0);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Integer));
            return CilDispatchResult.Success();
        }

        /// <inheritdoc />
        protected override StackSlot GetReturnValue(CilExecutionContext context, long dataAddress, TypeSignature targetType)
        {
            // TODO: Handle Nullable<T>

            if (targetType.IsValueType)
            {
                // If it is a value type, read the structure in the boxed object.
                var value = context.Machine.ValueFactory.RentValue(targetType, false);
                context.Machine.Memory.Read(dataAddress, value);
                return new StackSlot(value, StackSlotTypeHint.Structure);
            }
            else
            {
                // If it is a reference type, unbox.any == castclass, i.e., just push the object pointer.
                var value = context.Machine.ValueFactory.RentNativeInteger(dataAddress - context.Machine.ValueFactory.ObjectHeaderSize);
                return new StackSlot(value, StackSlotTypeHint.Integer);
            }
        }
    }
}