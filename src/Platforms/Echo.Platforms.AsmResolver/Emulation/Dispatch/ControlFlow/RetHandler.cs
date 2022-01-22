using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    [DispatcherTableEntry(CilCode.Ret)]
    public class RetHandler : ICilOpCodeHandler
    {
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            context.Machine.CallStack.Pop();
            
            // TODO: Push return value onto the caller's evaluation stack.
            
            return CilDispatchResult.Success();
        }
    }
}