namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides an empty base implementation for a block listener.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions in the blocks.</typeparam>
    public abstract class BlockListenerBase<TInstruction> : IBlockListener<TInstruction>
    {
        /// <inheritdoc />
        public virtual void VisitBasicBlock(BasicBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void EnterScopeBlock(ScopeBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void ExitScopeBlock(ScopeBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void EnterExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void ExitExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void EnterProtectedBlock(ExceptionHandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void ExitProtectedBlock(ExceptionHandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void EnterHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex)
        {
        }

        /// <inheritdoc />
        public virtual void ExitHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex)
        {
        }

        /// <inheritdoc />
        public virtual void EnterPrologueBlock(HandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void ExitPrologueBlock(HandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void EnterEpilogueBlock(HandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void ExitEpilogueBlock(HandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void EnterHandlerContents(HandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public virtual void ExitHandlerContents(HandlerBlock<TInstruction> block)
        {
        }
    }
}