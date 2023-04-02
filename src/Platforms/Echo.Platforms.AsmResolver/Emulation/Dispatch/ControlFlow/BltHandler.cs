using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>blt</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Blt, CilCode.Blt_S, CilCode.Blt_Un, CilCode.Blt_Un_S)]
    public class BltHandler : BinaryBranchHandlerBase
    {
        /// <inheritdoc />
        protected override bool IsSignedCondition(CilInstruction instruction) =>
            instruction.OpCode.Code is CilCode.Blt or CilCode.Blt_S;

        /// <inheritdoc />
        protected override Trilean EvaluateCondition(CilInstruction instruction, 
            StackSlot argument1,
            StackSlot argument2)
        {
            var contents1 = argument1.Contents.AsSpan();
            var contents2 = argument2.Contents.AsSpan();
            bool isSigned = IsSignedCondition(instruction);

            return argument1.TypeHint == StackSlotTypeHint.Integer
                ? contents1.IntegerIsLessThan(contents2, isSigned)
                : contents1.FloatIsLessThan(contents2, isSigned);
        }
    }
}