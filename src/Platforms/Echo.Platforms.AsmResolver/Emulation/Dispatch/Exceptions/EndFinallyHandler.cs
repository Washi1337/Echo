using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Exceptions
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>endfinally</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Endfinally)]
    public class EndFinallyHandler : ICilOpCodeHandler
    {
         /// <inheritdoc />
         public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction) 
         {
             var result = context.CurrentFrame.ExceptionHandlerStack.EndFinally();
             if (!result.IsSuccess)
                 return CilDispatchResult.Exception(result.ExceptionObject);
             
             context.CurrentFrame.ProgramCounter = result.NextOffset;
             return CilDispatchResult.Success();
         }
    }
}