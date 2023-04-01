using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a base for branch instructions that evaluate a condition using a unary operator.
    /// </summary>
    public abstract class UnaryBranchHandlerBase : BranchHandlerBase
    {
        /// <inheritdoc />
        protected override bool EvaluateCondition(CilExecutionContext context, CilInstruction instruction)
        {
            var value = context.CurrentFrame.EvaluationStack.Pop();
            var result = EvaluateCondition(value);

            if (result.IsUnknown)
                result = context.Machine.UnknownResolver.ResolveBranchCondition(context, instruction, value);
            
            context.Machine.ValueFactory.BitVectorPool.Return(value.Contents);
            
            return result.ToBoolean();
        }

        /// <summary>
        /// Evaluates the unary condition on the provided stack slot.
        /// </summary>
        /// <param name="argument">The stack slot.</param>
        /// <returns>The result of the evaluation, or <c>null</c> if the evaluation failed due to an invalid program.</returns>
        protected abstract Trilean EvaluateCondition(StackSlot argument);
    }
}