namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides members for listening to events raised by the <see cref="BlockWalker{TInstruction}"/> class. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions in the blocks.</typeparam>
    public interface IBlockListener<TInstruction>
    {
        /// <summary>
        /// Visits a basic block.
        /// </summary>
        /// <param name="block">The block.</param>
        void VisitBasicBlock(BasicBlock<TInstruction> block);

        /// <summary>
        /// Enters a scope block.
        /// </summary>
        /// <param name="block">The block.</param>
        void EnterScopeBlock(ScopeBlock<TInstruction> block);
        
        /// <summary>
        /// Exits a scope block.
        /// </summary>
        /// <param name="block">The block.</param>
        void ExitScopeBlock(ScopeBlock<TInstruction> block);

        /// <summary>
        /// Enters an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void EnterExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block);
        
        /// <summary>
        /// Exits an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void ExitExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block);
        
        /// <summary>
        /// Enters the protected region of an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void EnterProtectedBlock(ExceptionHandlerBlock<TInstruction> block);
        
        /// <summary>
        /// Exits the protected region of an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void ExitProtectedBlock(ExceptionHandlerBlock<TInstruction> block);

        /// <summary>
        /// Enters the prologue region of an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void EnterPrologueBlock(ExceptionHandlerBlock<TInstruction> block);
        
        /// <summary>
        /// Exits the prologue region of an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void ExitPrologueBlock(ExceptionHandlerBlock<TInstruction> block);

        /// <summary>
        /// Enters a handler region of an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="handlerIndex">The index of the handler that was entered.</param>
        void EnterHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex);
        
        /// <summary>
        /// Exits a handler region of an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="handlerIndex">The index of the handler that was exit.</param>
        void ExitHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex);

        /// <summary>
        /// Enters the epilogue region of an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void EnterEpilogueBlock(ExceptionHandlerBlock<TInstruction> block);
        
        /// <summary>
        /// Exits the epilogue region of an exception handler block.
        /// </summary>
        /// <param name="block">The block.</param>
        void ExitEpilogueBlock(ExceptionHandlerBlock<TInstruction> block);
    }
}