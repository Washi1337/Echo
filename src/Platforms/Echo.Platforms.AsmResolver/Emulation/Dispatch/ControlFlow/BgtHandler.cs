using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>bgt</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Bgt, CilCode.Bgt_S, CilCode.Bgt_Un, CilCode.Bgt_Un_S)]
    public class BgtHandler : BinaryBranchHandlerBase
    {
        /// <inheritdoc />
        protected override bool IsSignedCondition(CilInstruction instruction) =>
            instruction.OpCode.Code is CilCode.Bgt or CilCode.Bgt_S;

        /// <inheritdoc />
        protected override Trilean EvaluateCondition(CilInstruction instruction, 
            StackSlot argument1,
            StackSlot argument2)
        {
            var contents1 = argument1.Contents.AsSpan();
            var contents2 = argument2.Contents.AsSpan();
            bool isSigned = IsSignedCondition(instruction);

            return argument1.TypeHint == StackSlotTypeHint.Integer
                ? contents1.IntegerIsGreaterThan(contents2, isSigned)
                : contents1.FloatIsGreaterThan(contents2, isSigned);
        }
    }
}