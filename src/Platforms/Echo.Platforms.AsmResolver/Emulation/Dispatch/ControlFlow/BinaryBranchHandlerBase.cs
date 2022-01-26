using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a base for branch instructions that evaluate a condition using a binary operator.
    /// </summary>
    public abstract class BinaryBranchHandlerBase : BranchHandlerBase
    {
        /// <inheritdoc />
        protected override Trilean? EvaluateCondition(CilExecutionContext context, CilInstruction instruction)
        {
            var pool = context.Machine.ValueFactory.BitVectorPool;
            
            // Pop arguments.
            var (argument1, argument2) = OperatorHelper.PopBinaryArguments(context, IsSignedCondition(instruction));

            // Evaluate.
            Trilean? result = argument1.TypeHint == argument2.TypeHint
                ? EvaluateCondition(instruction, argument1, argument2)
                : null;

            // Reuse stack bitvectors.
            pool.Return(argument1.Contents);
            pool.Return(argument2.Contents);

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the instruction is a signed operation or not.
        /// </summary>
        /// <param name="instruction">The instruction to classify.</param>
        /// <returns><c>true</c> if signed, <c>false</c> otherwise.</returns>
        protected abstract bool IsSignedCondition(CilInstruction instruction);

        /// <summary>
        /// Evaluates the binary condition on the provided stack arguments.
        /// </summary>
        /// <param name="instruction">The instruction containing the operator to evaluate.</param>
        /// <param name="argument1">The left hand side of the binary operator.</param>
        /// <param name="argument2">The right hand side of the binary operator.</param>
        /// <returns>The result of the evaluation, or <c>null</c> if the evaluation failed due to an invalid program.</returns>
        protected abstract Trilean EvaluateCondition(CilInstruction instruction, StackSlot argument1, StackSlot argument2);
    }
}