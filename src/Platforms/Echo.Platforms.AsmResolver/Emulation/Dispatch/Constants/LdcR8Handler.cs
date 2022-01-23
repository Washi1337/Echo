using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    [DispatcherTableEntry(CilCode.Ldc_R8)]
    public class LdcR8Handler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var value = context.Machine.ValueFactory.BitVectorPool.Rent(64, false);
            var span = value.AsSpan();
            span.F64 = (double) instruction.Operand!;
            span.MarkFullyKnown();
            
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Float));
            
            return CilDispatchResult.Success();
        }
    }
}