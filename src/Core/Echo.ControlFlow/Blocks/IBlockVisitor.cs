namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides members for visiting blocks in a scoped block tree. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions in the blocks.</typeparam>
    public interface IBlockVisitor<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Visits a basic block.
        /// </summary>
        /// <param name="block">The block.</param>
        void VisitBasicBlock(BasicBlock<TInstruction> block);
        
        /// <summary>
        /// Visits a scope block.
        /// </summary>
        /// <param name="block">The block.</param>
        void VisitScopeBlock(ScopeBlock<TInstruction> block);

        /// <summary>
        /// Visits an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void VisitExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block);

        /// <summary>
        /// Visits a handler block inside an <see cref="ExceptionHandlerBlock{TInstruction}"/>.
        /// </summary>
        /// <param name="block">The block.</param>
        void VisitHandlerBlock(HandlerBlock<TInstruction> block);
    }

    /// <summary>
    /// Provides members for visiting blocks in a scoped block tree. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions in the blocks.</typeparam>
    /// <typeparam name="TState">The type of state to pass onto the visitor.</typeparam>
    /// <typeparam name="TResult">The type of the result for every visited block.</typeparam>
    public interface IBlockVisitor<TInstruction, in TState, out TResult>
        where TInstruction : notnull
    {
        /// <summary>
        /// Visits a basic block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="state">The argument to pass along the visitor.</param>
        TResult VisitBasicBlock(BasicBlock<TInstruction> block, TState state);
        
        /// <summary>
        /// Visits a scope block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="state">The argument to pass along the visitor.</param>
        TResult VisitScopeBlock(ScopeBlock<TInstruction> block, TState state);

        /// <summary>
        /// Visits an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="state">The argument to pass along the visitor.</param>
        TResult VisitExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block, TState state);

        /// <summary>
        /// Visits a handler block inside an <see cref="ExceptionHandlerBlock{TInstruction}"/>.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="state">The argument to pass along the visitor.</param>
        TResult VisitHandlerBlock(HandlerBlock<TInstruction> block, TState state);
    }

}