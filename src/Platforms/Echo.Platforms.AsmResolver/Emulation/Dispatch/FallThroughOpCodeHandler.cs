using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides a base for instruction handlers that finalize evaluation by increasing the program counter by the
    /// instruction's size. 
    /// </summary>
    public abstract class FallThroughOpCodeHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var result = DispatchInternal(context, instruction);
            if (result.IsSuccess)
                context.CurrentFrame.ProgramCounter += instruction.Size;
            return result;
        }

        /// <summary>
        /// Evaluates a CIL instruction in the provided execution context, without increasing the program counter.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="instruction">The instruction to dispatch and evaluate.</param>
        /// <returns>A value indicating whether the dispatch was successful or caused an error.</returns>
        protected abstract CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction);
    }
}