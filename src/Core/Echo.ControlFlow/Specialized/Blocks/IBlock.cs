using System.Collections.Generic;

namespace Echo.ControlFlow.Specialized.Blocks
{
    public interface IBlock<TInstruction>
    {
        IEnumerable<BasicBlock<TInstruction>> GetAllBlocks();
    }
}