using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers;

[DispatcherTableEntry(CilCode.Volatile)]
public class VolatileHandler : FallThroughOpCodeHandler
{
    /// <inheritdoc />
    protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
    {
        // Current virtual memory model does not distinguish between volatile and non-volatile read/writes, so this
        // opcode prefix is just a NOP for now.
        
        return CilDispatchResult.Success();
    }
}