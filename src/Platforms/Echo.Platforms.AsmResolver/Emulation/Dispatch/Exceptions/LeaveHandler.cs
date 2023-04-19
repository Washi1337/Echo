using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Exceptions
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>leave</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Leave, CilCode.Leave_S)]
    public class LeaveHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            int targetOffset = ((ICilLabel) instruction.Operand!).Offset;
            
            // Leave the EH, and jump to the next offset.
            context.CurrentFrame.EvaluationStack.Clear();
            context.CurrentFrame.ProgramCounter = context.CurrentFrame.ExceptionHandlerStack.Leave(targetOffset);

            return CilDispatchResult.Success();
        }
    }
}