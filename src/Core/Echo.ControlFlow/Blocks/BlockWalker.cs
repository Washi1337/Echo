using System;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides a mechanism for traversing a scoped block tree in order. 
    /// </summary>
    /// <typeparam name="TInstruction"></typeparam>
    public class BlockWalker<TInstruction> : IBlockVisitor<TInstruction>
        where TInstruction : notnull
    {
        private readonly IBlockListener<TInstruction> _listener;

        /// <summary>
        /// Creates a new block walker.
        /// </summary>
        /// <param name="listener">The object that responds to traversal events.</param>
        public BlockWalker(IBlockListener<TInstruction> listener)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
        }
        
        /// <summary>
        /// Traverses a block tree and notifies the listener with traversal events.
        /// </summary>
        /// <param name="block">The root of the block tree to traverse.</param>
        public void Walk(IBlock<TInstruction> block) => block.AcceptVisitor(this);

        /// <inheritdoc />
        public void VisitBasicBlock(BasicBlock<TInstruction> block) => _listener.VisitBasicBlock(block);

        /// <inheritdoc />
        public void VisitScopeBlock(ScopeBlock<TInstruction> block)
        {
            _listener.EnterScopeBlock(block);
            for (int i = 0; i < block.Blocks.Count; i++)
                block.Blocks[i].AcceptVisitor(this);

            _listener.ExitScopeBlock(block);
        }

        /// <inheritdoc />
        public void VisitExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block)
        {
            _listener.EnterExceptionHandlerBlock(block);

            _listener.EnterProtectedBlock(block);
            block.ProtectedBlock.AcceptVisitor(this);
            _listener.ExitProtectedBlock(block);

            for (int i = 0; i < block.Handlers.Count; i++)
            {
                var handlerBlock = block.Handlers[i];
                
                _listener.EnterHandlerBlock(block, i);
                handlerBlock.AcceptVisitor(this);
                _listener.ExitHandlerBlock(block, i);
            }
            
            _listener.ExitExceptionHandlerBlock(block);
        }

        /// <inheritdoc />
        public void VisitHandlerBlock(HandlerBlock<TInstruction> block)
        {
            if (block.Prologue != null)
            {
                _listener.EnterPrologueBlock(block);
                block.Prologue.AcceptVisitor(this);
                _listener.ExitPrologueBlock(block);
            }

            _listener.EnterHandlerContents(block);
            block.Contents.AcceptVisitor(this);
            _listener.ExitHandlerContents(block);
            
            if (block.Epilogue != null)
            {
                _listener.EnterEpilogueBlock(block);
                block.Epilogue.AcceptVisitor(this);
                _listener.ExitEpilogueBlock(block);
            }
        }
        
    }
}