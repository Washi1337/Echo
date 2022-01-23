using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Provides a base for unary operator instruction handlers.
    /// </summary>
    public abstract class UnaryOpCodeHandlerBase : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var value = context.CurrentFrame.EvaluationStack.Pop();
            var result = Evaluate(context, instruction, value);
            context.CurrentFrame.EvaluationStack.Push(value);
            return result;
        }

        /// <summary>
        /// Evaluates the unary operation on an arguments. 
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="instruction">The instruction to dispatch and evaluate.</param>
        /// <param name="argument">The argument that also receives the output.</param>
        /// <returns>A value indicating whether the dispatch was successful or caused an error.</returns>
        protected abstract CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, StackSlot argument);
    }
}