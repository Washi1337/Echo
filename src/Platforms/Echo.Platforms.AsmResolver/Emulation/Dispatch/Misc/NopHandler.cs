using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Misc
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>nop</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Nop)]
    public class NopHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            return CilDispatchResult.Success();
        }
    }
}