using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
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

        protected abstract CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, StackSlot argument);
    }
}