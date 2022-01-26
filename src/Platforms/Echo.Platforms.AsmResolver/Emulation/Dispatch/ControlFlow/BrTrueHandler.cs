using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>brtrue</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Brtrue, CilCode.Brtrue_S)]
    public class BrTrueHandler : UnaryBranchHandlerBase
    {
        /// <inheritdoc />
        protected override Trilean? EvaluateCondition(StackSlot argument) => !argument.Contents.AsSpan().IsZero;
    }
}