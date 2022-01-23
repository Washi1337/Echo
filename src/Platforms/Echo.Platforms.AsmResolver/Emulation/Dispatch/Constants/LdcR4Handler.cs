using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldc.r4</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldc_R4)]
    public class LdcR4Handler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            // CLR pushes a 32-bit float as a 64-bit float on the stack!

            var value = context.Machine.ValueFactory.BitVectorPool.Rent(64, false);
            var span = value.AsSpan();
            span.F64 = (float) instruction.Operand!;
            span.MarkFullyKnown();
            
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Float));
            
            return CilDispatchResult.Success();
        }
    }
}