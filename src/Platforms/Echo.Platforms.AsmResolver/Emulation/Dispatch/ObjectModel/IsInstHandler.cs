using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>isinst</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Isinst)]
    public class IsInstHandler : CastOpCodeHandlerBase
    {
        /// <inheritdoc />
        protected override CilDispatchResult HandleFailedCast(CilExecutionContext context, TypeSignature originalType, TypeSignature targetType)
        {
            var value = context.Machine.ValueFactory.RentNativeInteger(0);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Integer));
            return CilDispatchResult.Success();
        }

        /// <inheritdoc />
        protected override CilDispatchResult HandleSuccessfulCast(CilExecutionContext context, long objectAddress, TypeSignature targetType)
        {
            var value = context.Machine.ValueFactory.RentNativeInteger(objectAddress);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Integer));
            return CilDispatchResult.Success();
        }
    }
}