using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Serialization.Blocks
{
    public class BlockSorterTest
    {
        private ControlFlowNode<DummyInstruction>[] GetSorting(DummyInstruction[] instructions)
        {
            var builder = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance, 
                instructions,
                DummyArchitecture.Instance.SuccessorResolver);

            var cfg = builder.ConstructFlowGraph(0);
            
            var sorter = new BlockSorter<DummyInstruction>(cfg);
            return sorter.GetSorting().ToArray();
        }
        
        [Fact]
        public void SequenceShouldStartWithEntrypointNode()
        {
            var sorting = GetSorting(new[]
            {
                DummyInstruction.Op(0, 0, 1),
                DummyInstruction.JmpCond(1, 4),
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Jmp(3, 5),
                DummyInstruction.Op(4, 0, 0),
                DummyInstruction.Ret(5), 
            });
            
            Assert.Equal(0, sorting[0].Offset);
        }
    }
}