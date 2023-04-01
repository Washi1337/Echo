using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>add</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Add, CilCode.Add_Ovf, CilCode.Add_Ovf_Un)]
    public class AddHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool Force32BitResult(CilInstruction instruction) => false;

        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction)
        {
            return instruction.OpCode.Code is CilCode.Add or CilCode.Add_Ovf;
        }

        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, 
            StackSlot argument1, StackSlot argument2)
        {
            var argument1Value = argument1.Contents.AsSpan();
            var argument2Value = argument2.Contents.AsSpan();
            
            if (argument1.TypeHint == StackSlotTypeHint.Integer)
                argument1Value.IntegerAdd(argument2Value);
            else
                argument1Value.FloatAdd(argument2Value);
            
            // TODO: overflow check.
            
            return CilDispatchResult.Success();
        }
    }
}