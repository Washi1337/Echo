using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>sub</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Sub, CilCode.Sub_Ovf, CilCode.Sub_Ovf_Un)]
    public class SubHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction)
        {
            return instruction.OpCode.Code is CilCode.Sub or CilCode.Sub_Ovf;
        }

        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, 
            StackSlot argument1, StackSlot argument2)
        {
            var argument1Value = argument1.Contents.AsSpan();
            var argument2Value = argument2.Contents.AsSpan();
            
            if (argument1.TypeHint == StackSlotTypeHint.Integer)
                argument1Value.IntegerSubtract(argument2Value);
            else
                argument1Value.FloatSubtract(argument2Value);
            
            // TODO: overflow check.
            
            return CilDispatchResult.Success();
        }
    }
}