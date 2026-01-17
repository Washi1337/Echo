using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>rem</c> operations and it derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Rem, CilCode.Rem_Un)]
    public class RemHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool Force32BitResult(CilInstruction instruction) => false;

        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction) => instruction.OpCode.Code == CilCode.Rem;

        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(
            CilExecutionContext context,
            CilInstruction instruction, 
            StackSlot argument1,
            StackSlot argument2)
        {
            var argument1Value = argument1.Contents.AsSpan();
            var argument2Value = argument2.Contents.AsSpan();
            
            if (argument1.TypeHint == StackSlotTypeHint.Integer)
                argument1Value.IntegerRemainder(argument2Value, IsSignedOperation(instruction));
            else
                argument1Value.FloatRemainder(argument2Value);
            
            // TODO: overflow check.
            
            return CilDispatchResult.Success();
        }
    }
}