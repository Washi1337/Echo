using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>cgt</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Cgt, CilCode.Cgt_Un)]
    public class CgtHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool Force32BitResult(CilInstruction instruction) => true;

        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction) => instruction.OpCode.Code == CilCode.Cgt;

        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(
            CilExecutionContext context, 
            CilInstruction instruction, 
            StackSlot argument1, 
            StackSlot argument2)
        {
            var argument1Span = argument1.Contents.AsSpan();

            bool isSigned = IsSignedOperation(instruction);

            var result = argument1.TypeHint == StackSlotTypeHint.Integer
                ? argument1Span.IntegerIsGreaterThan(argument2.Contents.AsSpan(), isSigned)
                : argument1Span.FloatIsGreaterThan(argument2.Contents.AsSpan(), isSigned);

            argument1Span.Clear();
            argument1Span[0] = result;
            
            return CilDispatchResult.Success();
        }
    }
}