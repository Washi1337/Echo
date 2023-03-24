using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
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

            var value = stack.Pop();
            var targetType = ((ITypeDefOrRef) instruction.Operand!).ToTypeSignature();

            try
            {
                if (value.TypeHint != StackSlotTypeHint.Integer)
                    return CilDispatchResult.InvalidProgram(context);

                var objectAddress = value.Contents.AsSpan();
                switch (objectAddress)
                {
                    case { IsFullyKnown: false }:
                        // TODO: Make configurable.
                        throw new CilEmulatorException("Attempted to type-cast an unknown pointer");
                    
                    case { IsZero.Value: TrileanValue.True }:
                        return HandleNull(context, targetType);
                    
                    default:
                        long actualAddress = objectAddress.ReadNativeInteger(context.Machine.Is32Bit);
                        var objectType = actualAddress.GetObjectPointerType(context.Machine).ToTypeSignature();

                        // TODO: handle full verifier-assignable-to operation.
                        return objectType.IsAssignableTo(targetType)
                            ? HandleSuccessfulCast(context, actualAddress, targetType)
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
            return CilDispatchResult.NullReference(context);
        }

        /// <summary>
        /// Handles the case when the pushed value is incompatible with the type specified in the instruction.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="originalType">The type of the pushed object.</param>
        /// <param name="targetType">The type that the object was attempted to be converted to.</param>
        /// <returns>The final dispatcher result.</returns>
        protected virtual CilDispatchResult HandleFailedCast(CilExecutionContext context, TypeSignature originalType, TypeSignature targetType)
        {
            return CilDispatchResult.InvalidCast(context, originalType, targetType);
        }

        /// <summary>
        /// Handles the case when the pushed value is compatible with the type specified in the instruction.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="objectAddress">The address of the object.</param>
        /// <param name="targetType">The type to convert the object to to.</param>
        /// <returns>The final dispatcher result.</returns>
        protected abstract CilDispatchResult HandleSuccessfulCast(CilExecutionContext context, long objectAddress, TypeSignature targetType);

    }
}