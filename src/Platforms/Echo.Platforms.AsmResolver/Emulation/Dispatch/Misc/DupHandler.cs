using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Misc
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>dup</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Dup)]
    public class DupHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var value = context.CurrentFrame.EvaluationStack.Peek();

            var copy = context.Machine.ValueFactory.BitVectorPool.Rent(value.Contents.Count, false);
            copy.AsSpan().Write(value.Contents);
            
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(copy, value.TypeHint));
            
            return CilDispatchResult.Success();
        }
    }
}