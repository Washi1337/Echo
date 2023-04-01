using Echo.Core;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>br</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Br, CilCode.Br_S)]
    public class BrHandler : BranchHandlerBase
    {
        /// <inheritdoc />
        protected override bool EvaluateCondition(CilExecutionContext context, CilInstruction instruction) => true;
    }
}