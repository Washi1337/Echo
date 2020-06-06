using System;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides a mechanism for traversing a scoped block tree in order. 
    /// </summary>
    /// <typeparam name="TInstruction"></typeparam>
    public class BlockWalker<TInstruction> : IBlockVisitor<TInstruction>
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
            foreach (var innerBlock in block.Blocks)
                innerBlock.AcceptVisitor(this);
            _listener.ExitScopeBlock(block);
        }
    }
}