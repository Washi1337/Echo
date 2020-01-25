using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Represents a collection of blocks grouped together into one single block. 
    /// </summary>
    /// <typeparam name="TInstruction"></typeparam>
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
        public IEnumerable<BasicBlock<TInstruction>> GetAllBlocks()
        {
            return Blocks.SelectMany(b => b.GetAllBlocks());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string newLine = Environment.NewLine;
            return "{" + newLine + string.Join(newLine, Blocks) + newLine + "}";
        }
    }
}