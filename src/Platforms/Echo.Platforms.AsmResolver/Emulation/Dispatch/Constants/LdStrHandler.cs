using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldstr</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldstr)]
    public class LdStrHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            string value = instruction.Operand!.ToString();

            long stringAddress = context.Machine.Heap.GetInternedString(value);
            var vector = context.Machine.ValueFactory.RentNativeInteger(stringAddress);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(vector, StackSlotTypeHint.Integer));
            
            return CilDispatchResult.Success();
        }
    }
}