using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldarga</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldarga, CilCode.Ldarga_S)]
    public class LdArgaHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.CurrentFrame;
            var factory = context.Machine.ValueFactory;

            // Extract parameter in opcode or operand.
            var parameter = instruction.GetParameter(frame.Body!.Owner.Parameters);

            // Push address on top of stack.
            long address = frame.GetArgumentAddress(parameter.Index);
            var vector = context.Machine.ValueFactory.RentNativeInteger(address);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(vector, StackSlotTypeHint.Integer));
            
            return CilDispatchResult.Success();
        }
    }
}