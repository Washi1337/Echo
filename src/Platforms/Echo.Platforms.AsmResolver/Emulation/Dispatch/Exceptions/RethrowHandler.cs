using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Exceptions
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>rethrow</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Rethrow)]
    public class RethrowHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            context.CurrentFrame.EvaluationStack.Clear();
            var exceptionObject = context.CurrentFrame.ExceptionHandlerStack.Peek().ExceptionObject;
            return CilDispatchResult.Exception(exceptionObject);
        }
    }
}