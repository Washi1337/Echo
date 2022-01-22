using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    public interface ICilOpCodeHandler
    {
        CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction);
    }
}