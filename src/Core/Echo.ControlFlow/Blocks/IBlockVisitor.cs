namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides members for visiting blocks in a scoped block tree. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions in the blocks.</typeparam>
    /// <typeparam name="TResult">The type of result to produce.</typeparam>
    public interface IBlockVisitor<TInstruction, out TResult>
    {
        /// <summary>
        /// Visits a basic block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns>The produced result.</returns>
        TResult VisitBasicBlock(BasicBlock<TInstruction> block);
        
        /// <summary>
        /// Visits a scope block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns>The produced result.</returns>
        TResult VisitScopeBlock(ScopeBlock<TInstruction> block);
    }
}