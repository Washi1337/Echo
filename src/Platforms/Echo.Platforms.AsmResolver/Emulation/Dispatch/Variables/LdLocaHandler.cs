using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldloca</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldloca, CilCode.Ldloca_S)]
    public class LdLocaHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.CurrentFrame;
            
            // Extract local variable in opcode or operand.
            var local = instruction.GetLocalVariable(frame.Body!.LocalVariables);

            // Push address on top of stack.
            long address = frame.GetLocalAddress(local.Index);
            var vector = context.Machine.ValueFactory.RentNativeInteger(address);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(vector, StackSlotTypeHint.Integer));
            
            return CilDispatchResult.Success();
        }
    }
}