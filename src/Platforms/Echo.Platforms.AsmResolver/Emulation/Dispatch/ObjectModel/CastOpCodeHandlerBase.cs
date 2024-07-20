using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a base for instructions implementing type-casting and boxing behavior.
    /// </summary>
    public abstract class CastOpCodeHandlerBase : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            var genericContext = GenericContext.FromMethod(context.CurrentFrame.Method);

            var value = stack.Pop();
            var targetType = ((ITypeDefOrRef) instruction.Operand!).ToTypeSignature().InstantiateGenericTypes(genericContext);

            try
            {
                if (value.TypeHint != StackSlotTypeHint.Integer)
                    return CilDispatchResult.InvalidProgram(context);

                long? objectAddress = value.Contents.IsFullyKnown 
                    ? value.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit) 
                    : context.Machine.UnknownResolver.ResolveSourcePointer(context, instruction, value);

                switch (objectAddress)
                {
                    case null:
                        // If address is unknown even after resolution, assume casting works but keep pointer unknown.
                        stack.Push(new StackSlot(value.Contents.Clone(factory.BitVectorPool), StackSlotTypeHint.Integer));
                        return CilDispatchResult.Success();

                    case 0:
                        // A null reference was passed.
                        return HandleNull(context, targetType);

                    case { } actualAddress:
                        // A non-null reference was passed.
                        var handle = actualAddress.AsObjectHandle(context.Machine);
                        var objectType = handle.GetObjectType().ToTypeSignature();

                        // TODO: handle full verifier-assignable-to operation.
                        return objectType.IsAssignableTo(targetType)
                            ? HandleSuccessfulCast(context, handle, targetType)
                            : HandleFailedCast(context, objectType, targetType);
                }
            }
            finally
            {
                factory.BitVectorPool.Return(value.Contents);
            }
        }

        /// <summary>
        /// Handles the case when the pushed value is <c>null</c>.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="targetType">The type to convert the null reference to.</param>
        /// <returns>The final dispatcher result.</returns>
        protected virtual CilDispatchResult HandleNull(CilExecutionContext context, TypeSignature targetType)
        {
            var value = context.Machine.ValueFactory.RentNull();
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Integer));
            return CilDispatchResult.Success();
        }

        /// <summary>
        /// Handles the case when the pushed value is incompatible with the type specified in the instruction.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="originalType">The type of the pushed object.</param>
        /// <param name="targetType">The type that the object was attempted to be converted to.</param>
        /// <returns>The final dispatcher result.</returns>
        protected virtual CilDispatchResult HandleFailedCast(
            CilExecutionContext context,
            TypeSignature originalType,
            TypeSignature targetType)
        {
            return CilDispatchResult.InvalidCast(context, originalType, targetType);
        }

        /// <summary>
        /// Handles the case when the pushed value is compatible with the type specified in the instruction.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="handle">The address of the object.</param>
        /// <param name="targetType">The type to convert the object to to.</param>
        /// <returns>The final dispatcher result.</returns>
        protected abstract CilDispatchResult HandleSuccessfulCast(
            CilExecutionContext context,
            ObjectHandle handle, 
            TypeSignature targetType);

    }
}