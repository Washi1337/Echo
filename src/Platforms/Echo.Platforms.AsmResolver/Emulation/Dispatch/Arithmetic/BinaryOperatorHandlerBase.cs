using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Provides a base for binary operator instruction handlers.
    /// </summary>
    public abstract class BinaryOperatorHandlerBase : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        { 
            var pool = context.Machine.ValueFactory.BitVectorPool;
            
            var (argument1, argument2) = OperatorHelper.PopBinaryArguments(context, IsSignedOperation(instruction));
            if (argument1.TypeHint != argument2.TypeHint)
            {
                // Return both arguments to the pool, as they won't be used anymore.
                pool.Return(argument1.Contents);
                pool.Return(argument2.Contents);

                return CilDispatchResult.InvalidProgram(context);
            }

            // Evaluate the operation!
            var result = Evaluate(context, instruction, argument1, argument2);
            
            // Release bitvector for arg 2 because it is not used any more.
            pool.Return(argument2.Contents);
            
            // Resize to 32bit if necessary.
            if (Force32BitResult(instruction) && argument1.Contents.Count != 32)
            {
                var resized = new StackSlot(argument1.Contents.Resize(32, false, pool), argument1.TypeHint);
                pool.Return(argument1.Contents);
                argument1 = resized;
            }
            
            // Push the result back onto the stack.
            context.CurrentFrame.EvaluationStack.Push(argument1);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the instruction always pushes a 32-bit value.
        /// </summary>
        /// <param name="instruction">The instruction to classify.</param>
        /// <returns><c>true</c> if a 32-bit value is always pushed, <c>false</c> otherwise.</returns>
        protected abstract bool Force32BitResult(CilInstruction instruction);

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