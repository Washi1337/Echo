using System.Linq;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Serialization.Blocks
{
    public class BlockBuilderTest
    {
        [Fact]
        public void FlatGraphShouldProduceFlatBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Ret(4),
            };

            var cfgBuilder = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                DummyArchitecture.Instance.SuccessorResolver);

            var cfg = cfgBuilder.ConstructFlowGraph(0);
            
            var blockBuilder = new BlockBuilder<DummyInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(cfg);
            
            Assert.Single(rootScope.Blocks);
            Assert.IsAssignableFrom<BasicBlock<DummyInstruction>>(rootScope.Blocks[0]);
            Assert.Equal(instructions, ((BasicBlock<DummyInstruction>) rootScope.Blocks[0]).Instructions);
        }
        
        [Fact]
        public void BasicRegionShouldTranslateToSingleScopeBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.JmpCond(1, 4),
                
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Jmp(3, 4),
                
                DummyInstruction.Op(4, 0, 0),
                DummyInstruction.Ret(5),
            };

            var cfgBuilder = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                DummyArchitecture.Instance.SuccessorResolver);

            var cfg = cfgBuilder.ConstructFlowGraph(0);
            
            var region = new BasicControlFlowRegion<DummyInstruction>();
            cfg.Regions.Add(region);
            region.Nodes.Add(cfg.Nodes[2]);
            
            var blockBuilder = new BlockBuilder<DummyInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(cfg);
            
            Assert.Equal(3, rootScope.Blocks.Count);
            Assert.IsAssignableFrom<BasicBlock<DummyInstruction>>(rootScope.Blocks[0]);
            Assert.IsAssignableFrom<ScopeBlock<DummyInstruction>>(rootScope.Blocks[1]);
            Assert.IsAssignableFrom<BasicBlock<DummyInstruction>>(rootScope.Blocks[2]);
        }
    }
}