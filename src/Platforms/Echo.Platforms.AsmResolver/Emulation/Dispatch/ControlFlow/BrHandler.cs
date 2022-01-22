using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    [DispatcherTableEntry(CilCode.Br)]
    public class BrHandler : BranchHandlerBase
    {
        /// <inheritdoc />
        protected override bool EvaluateCondition(CilExecutionContext context, CilInstruction instruction) => true;
    }
}