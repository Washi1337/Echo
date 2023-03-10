using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>shl</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Shl)]
    public class ShlHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction) => false;

        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(
            CilExecutionContext context, 
            CilInstruction instruction, 
            StackSlot argument1, 
            StackSlot argument2)
        {
            if (!argument2.Contents.AsSpan().IsFullyKnown)
                argument1.Contents.AsSpan().MarkFullyUnknown();
            else
                argument1.Contents.AsSpan().ShiftLeft(argument2.Contents.AsSpan().I32);

            return CilDispatchResult.Success();
        }
    }
}