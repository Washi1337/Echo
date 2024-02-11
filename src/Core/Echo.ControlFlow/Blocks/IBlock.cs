using System.Collections.Generic;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Represents a single block in structured program code. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that this block contains.</typeparam>
    public interface IBlock<TInstruction>
    {
        /// <summary>
        /// Gets an ordered collection of all basic blocks that can be found in this block.
        /// </summary>
        /// <returns>The ordered basic blocks.</returns>
        IEnumerable<BasicBlock<TInstruction>> GetAllBlocks();

        /// <summary>
        /// Gets the first basic block that appears in the ordered list of blocks. 
        /// </summary>
        /// <returns>The first basic block, or <c>null</c> if the block contains no basic blocks..</returns>
        BasicBlock<TInstruction> GetFirstBlock();

        /// <summary>
        /// Gets the last basic block that appears in the ordered list of blocks. 
        /// </summary>
        /// <returns>The last basic block, or <c>null</c> if the block contains no basic blocks..</returns>
        BasicBlock<TInstruction> GetLastBlock();

        /// <summary>
        /// Visit the current block using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        void AcceptVisitor(IBlockVisitor<TInstruction> visitor);

        /// <summary>
        /// Visit the current block using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <param name="state">An argument to pass onto the visitor.</param>
        TResult AcceptVisitor<TState, TResult>(IBlockVisitor<TInstruction, TState, TResult> visitor, TState state);
    }
}