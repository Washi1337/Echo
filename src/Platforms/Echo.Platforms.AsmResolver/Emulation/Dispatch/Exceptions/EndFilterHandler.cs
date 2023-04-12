using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Exceptions
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>endfilter</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Endfilter)]
    public class EndFilterHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var evalStack = context.CurrentFrame.EvaluationStack;
            var ehStack = context.CurrentFrame.ExceptionHandlerStack;

            var filterResult = evalStack.Pop().Contents;
            try
            {
                var exception = ehStack.Peek().ExceptionObject;
                
                // Attempt to handle the filter.
                var result = ehStack.EndFilter(!filterResult.AsSpan().IsZero.ToBoolean());
                if (!result.IsSuccess)
                    return CilDispatchResult.Exception(result.ExceptionObject);

                // We passed the filter, jump to the actual handler with the exception on top of the stack.
                context.CurrentFrame.ProgramCounter = result.NextOffset;
                evalStack.Push(exception);
                
                return CilDispatchResult.Success();
            }
            finally
            {
                context.Machine.ValueFactory.BitVectorPool.Return(filterResult);
            }
        }
    }
}