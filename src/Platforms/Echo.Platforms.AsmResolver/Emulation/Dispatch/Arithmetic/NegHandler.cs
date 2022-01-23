using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    [DispatcherTableEntry(CilCode.Neg)]
    public class NegHandler : UnaryOpCodeHandlerBase
    {
        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, StackSlot argument)
        {
            if (argument.TypeHint == StackSlotTypeHint.Integer)
                argument.Contents.AsSpan().IntegerNegate();
            else
                argument.Contents.AsSpan().FloatNegate();
            
            return CilDispatchResult.Success();
        }
    }
}