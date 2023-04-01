using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>div</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Div, CilCode.Div_Un)]
    public class DivHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool Force32BitResult(CilInstruction instruction) => false;
        
        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction) => instruction.OpCode.Code == CilCode.Div;

        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, StackSlot argument1,
            StackSlot argument2)
        {
            var argument1Value = argument1.Contents.AsSpan();
            var argument2Value = argument2.Contents.AsSpan();
            
            if (argument1.TypeHint == StackSlotTypeHint.Integer)
                argument1Value.IntegerDivide(argument2Value);
            else
                argument1Value.FloatDivide(argument2Value);
            
            // TODO: overflow check.
            
            return CilDispatchResult.Success();
        }
    }

}