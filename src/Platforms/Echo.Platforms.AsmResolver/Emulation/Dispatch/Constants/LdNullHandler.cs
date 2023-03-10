using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldnull</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldnull)]
    public class LdNullHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var value = context.Machine.ValueFactory.BitVectorPool.RentNativeInteger(context.Machine.Is32Bit, true);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Integer));
            return CilDispatchResult.Success();   
        }
    }
}