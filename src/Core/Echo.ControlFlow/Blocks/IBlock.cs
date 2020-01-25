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
    }
}