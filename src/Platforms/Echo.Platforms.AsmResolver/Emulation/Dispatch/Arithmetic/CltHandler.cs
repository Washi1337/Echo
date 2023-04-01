using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>clt</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Clt, CilCode.Clt_Un)]
    public class CltHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool Force32BitResult(CilInstruction instruction) => true;

        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction) => instruction.OpCode.Code == CilCode.Clt;

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
                ? argument1Span.IntegerIsLessThan(argument2.Contents.AsSpan(), isSigned)
                : argument1Span.FloatIsLessThan(argument2.Contents.AsSpan(), isSigned);

            argument1Span.Clear();
            argument1Span[0] = result;
            
            return CilDispatchResult.Success();
        }
    }
}