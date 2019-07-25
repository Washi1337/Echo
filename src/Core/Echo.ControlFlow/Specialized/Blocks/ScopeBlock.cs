using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Specialized.Blocks
{
    public class ScopeBlock<TInstruction> : IBlock<TInstruction>
    {
        public IList<IBlock<TInstruction>> Blocks
        {
            get;
        } = new List<IBlock<TInstruction>>();

        public IEnumerable<BasicBlock<TInstruction>> GetAllBlocks()
        {
            return Blocks.SelectMany(b => b.GetAllBlocks());
        }

        public override string ToString()
        {
            string newLine = Environment.NewLine;
            return "{" + newLine + string.Join(newLine, Blocks) + newLine + "}";
        }
    }
}