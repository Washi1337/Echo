using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldsflda</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldsflda)]
    public class LdsFldaHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var field = (IFieldDescriptor) instruction.Operand!;
            var address = context.Machine.ValueFactory.RentNativeInteger(
                context.Machine.StaticFields.GetFieldAddress(field));
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(address, StackSlotTypeHint.Integer));
            return CilDispatchResult.Success();
        }
    }
}