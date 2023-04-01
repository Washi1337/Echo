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

            var copy = new StackSlot(value.Contents.Clone(context.Machine.ValueFactory.BitVectorPool), value.TypeHint);
            context.CurrentFrame.EvaluationStack.Push(copy);
            
            return CilDispatchResult.Success();
        }
    }
}