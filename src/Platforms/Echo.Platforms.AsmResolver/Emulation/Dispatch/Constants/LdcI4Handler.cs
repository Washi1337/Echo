using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldc.i4</c> operations.
    /// </summary>
    [DispatcherTableEntry(
        CilCode.Ldc_I4, CilCode.Ldc_I4_0, CilCode.Ldc_I4_1, CilCode.Ldc_I4_2,
        CilCode.Ldc_I4_3, CilCode.Ldc_I4_4, CilCode.Ldc_I4_5, CilCode.Ldc_I4_6,
        CilCode.Ldc_I4_7, CilCode.Ldc_I4_8, CilCode.Ldc_I4_S, CilCode.Ldc_I4_M1)]
    public class LdcI4Handler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var value = context.Machine.ValueFactory.BitVectorPool.Rent(32, false);
            value.AsSpan().Write(instruction.GetLdcI4Constant());

            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Integer));
            
            return CilDispatchResult.Success();
        }
    }
}