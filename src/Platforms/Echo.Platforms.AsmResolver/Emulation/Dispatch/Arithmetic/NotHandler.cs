using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>not</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Not)]
    public class NotHandler : UnaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(
            CilExecutionContext context, 
            CilInstruction instruction,
            StackSlot argument)
        {
            argument.Contents.AsSpan().Not();
            return CilDispatchResult.Success();
        }
    }
}