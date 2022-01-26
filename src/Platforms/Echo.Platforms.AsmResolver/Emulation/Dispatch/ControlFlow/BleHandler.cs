using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ble</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ble, CilCode.Ble_S, CilCode.Ble_Un, CilCode.Ble_Un_S)]
    public class BleHandler : BinaryBranchHandlerBase
    {
        /// <inheritdoc />
        protected override bool IsSignedCondition(CilInstruction instruction) =>
            instruction.OpCode.Code is CilCode.Ble or CilCode.Ble_S;

        /// <inheritdoc />
        protected override Trilean EvaluateCondition(CilInstruction instruction, 
            StackSlot argument1,
            StackSlot argument2)
        {
            var contents1 = argument1.Contents.AsSpan();
            var contents2 = argument2.Contents.AsSpan();

            if (contents1.IsEqualTo(contents2))
                return true;
            
            bool isSigned = IsSignedCondition(instruction);
            return argument1.TypeHint == StackSlotTypeHint.Integer
                ? contents1.IntegerIsLessThan(contents2, isSigned)
                : contents1.FloatIsLessThan(contents2, isSigned);
        }
    }
}