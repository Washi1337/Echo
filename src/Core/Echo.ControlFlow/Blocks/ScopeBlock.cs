using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Represents a collection of blocks grouped together into one single block. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that this block contains.</typeparam>
    public class ScopeBlock<TInstruction> : IBlock<TInstruction>
    {
        /// <summary>
        /// Gets an ordered, mutable collection of blocks that are present in this scope.
        /// </summary>
        public IList<IBlock<TInstruction>> Blocks
        {
            get;
        } = new List<IBlock<TInstruction>>();

        /// <inheritdoc />
        public IEnumerable<BasicBlock<TInstruction>> GetAllBlocks() => 
            Blocks.SelectMany(b => b.GetAllBlocks());

        /// <inheritdoc />
        public BasicBlock<TInstruction> GetFirstBlock() => Blocks.Count > 0 
            ? Blocks[0].GetFirstBlock() 
            : null;

        /// <inheritdoc />
        public BasicBlock<TInstruction> GetLastBlock() => Blocks.Count > 0
            ? Blocks[Blocks.Count - 1].GetLastBlock()
            : null;

        /// <inheritdoc />
        public void AcceptVisitor(IBlockVisitor<TInstruction> visitor) => visitor.VisitScopeBlock(this);

        /// <inheritdoc />
        public TResult AcceptVisitor<TState, TResult>(IBlockVisitor<TInstruction, TState, TResult> visitor, TState state) 
            => visitor.VisitScopeBlock(this, state);

        /// <inheritdoc />
        public override string ToString() => BlockFormatter<TInstruction>.Format(this);
    }
}