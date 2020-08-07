namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides members for visiting blocks in a scoped block tree. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions in the blocks.</typeparam>
    public interface IBlockVisitor<TInstruction>
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
    }
}