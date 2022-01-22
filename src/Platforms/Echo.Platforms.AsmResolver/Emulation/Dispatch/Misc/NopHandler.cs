using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Misc
{
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