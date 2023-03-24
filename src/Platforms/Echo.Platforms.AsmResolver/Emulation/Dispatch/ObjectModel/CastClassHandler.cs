using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>castclass</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Castclass)]
    public class CastClassHandler : CastOpCodeHandlerBase
    {
        /// <inheritdoc />
        protected override CilDispatchResult HandleSuccessfulCast(CilExecutionContext context, long objectAddress, TypeSignature targetType)
        {
            var value = context.Machine.ValueFactory.RentNativeInteger(objectAddress);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Integer));
            return CilDispatchResult.Success();
        }
    }
}