using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Provides a base for binary operator instruction handlers.
    /// </summary>
    public abstract class BinaryOpCodeHandlerBase : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var argument2 = context.CurrentFrame.EvaluationStack.Pop();
            var argument1 = context.CurrentFrame.EvaluationStack.Pop();

            // If the types of the stack slots do not match, the CLR throws an InvalidOperationException.
            if (argument1.TypeHint != argument2.TypeHint)
                return CilDispatchResult.InvalidProgram(context);

            // Resize bitvectors if required.
            var pool = context.Machine.ValueFactory.BitVectorPool;
            if (argument1.Contents.Count != argument2.Contents.Count)
            {
                bool signed = IsSignedOperation(instruction);

                if (argument1.Contents.Count < argument2.Contents.Count)
                {
                    var newArgument1 = new StackSlot(
                        argument1.Contents.Resize(argument2.Contents.Count, signed, pool),
                        StackSlotTypeHint.Integer);
                    pool.Return(argument1.Contents);
                    argument1 = newArgument1;
                }
                else
                {
                    var newArgument2 = new StackSlot(
                        argument2.Contents.Resize(argument1.Contents.Count, signed, pool),
                        StackSlotTypeHint.Integer);
                    pool.Return(argument2.Contents);
                    argument2 = newArgument2;
                }
            }
            
            // Evaluate the opeartion!
            var result = Evaluate(context, instruction, argument1, argument2);
            
            // Release bitvector for arg 2 because it is not used any more. 
            pool.Return(argument2.Contents);
            
            // Push the result back onto the stack.
            context.CurrentFrame.EvaluationStack.Push(argument1);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the instruction is a signed operation or not.
        /// </summary>
        /// <param name="instruction">The instruction to classify.</param>
        /// <returns><c>true</c> if signed, <c>false</c> otherwise.</returns>
        protected abstract bool IsSignedOperation(CilInstruction instruction);

        /// <summary>
        /// Evaluates the binary operation on two arguments. 
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="instruction">The instruction to dispatch and evaluate.</param>
        /// <param name="argument1">The first argument that also receives the output.</param>
        /// <param name="argument2">The second argument.</param>
        /// <returns>A value indicating whether the dispatch was successful or caused an error.</returns>
        protected abstract CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, 
            StackSlot argument1, StackSlot argument2);
    }
}