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
        void ExitScopeBlock(ScopeBlock<TInstruction> block);
    }
}