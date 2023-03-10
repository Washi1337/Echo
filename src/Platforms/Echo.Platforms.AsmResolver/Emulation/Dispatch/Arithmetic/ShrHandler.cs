using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>shr</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Shr, CilCode.Shr_Un)]
    public class ShrHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction) => instruction.OpCode.Code == CilCode.Shr;

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
                argument1.Contents.AsSpan().ShiftRight(argument2.Contents.AsSpan().I32, IsSignedOperation(instruction));

            return CilDispatchResult.Success();
        }
    }
}