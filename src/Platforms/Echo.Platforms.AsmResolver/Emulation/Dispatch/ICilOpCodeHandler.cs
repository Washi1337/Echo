using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides members for evaluating an instruction.
    /// </summary>
    public interface ICilOpCodeHandler
    {
        /// <summary>
        /// Evaluates a CIL instruction in the provided execution context.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="instruction">The instruction to dispatch and evaluate.</param>
        /// <returns>A value indicating whether the dispatch was successful or caused an error.</returns>
        CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction);
    }
}