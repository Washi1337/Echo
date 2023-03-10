using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Misc
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>pop</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Pop)]
    public class PopHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var value = context.CurrentFrame.EvaluationStack.Pop();
            context.Machine.ValueFactory.BitVectorPool.Return(value.Contents);
            return CilDispatchResult.Success();
        }
    }
}