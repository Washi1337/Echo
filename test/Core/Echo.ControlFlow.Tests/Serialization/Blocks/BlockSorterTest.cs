using System;
using System.Linq;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.Platforms.DummyPlatform;
using Xunit;

namespace Echo.ControlFlow.Tests.Serialization.Blocks
{
    public class BlockSorterTest
    {
        private static ControlFlowGraph<int> GenerateGraph(int nodeCount)
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);
            for (int i =0 ; i < nodeCount;i++)
                cfg.Nodes.Add(new ControlFlowNode<int>(i,i));
            cfg.Entrypoint = cfg.Nodes[0];
            return cfg;
        }

        private static ControlFlowNode<int>[] GetSorting(ControlFlowGraph<int> cfg)
        {
            var sorter = new BlockSorter<int>();
            return sorter.GetSorting(cfg).ToArray();
        }

        private static void AssertHasSubSequence(ControlFlowNode<int>[] ordering, params int[] subSequence)
        {
            var cfg = ordering[0].ParentGraph;
            int index = Array.IndexOf(ordering, cfg.Nodes[subSequence[0]]);
            Assert.NotEqual(-1, index);

            for (int i = 0; i < subSequence.Length; i++)
                Assert.Equal(cfg.Nodes[subSequence[i]], ordering[i + index]);
        }

        private static void AssertHasCombination(ControlFlowNode<int>[] ordering, params int[] combination)
        {
            var cfg = ordering[0].ParentGraph;

            int minIndex = int.MaxValue;
            int maxIndex = int.MinValue;
            foreach (int id in combination)
            {
                int index = Array.IndexOf(ordering, cfg.Nodes[id]);
                Assert.NotEqual(-1, index);
                minIndex = Math.Min(index, minIndex);
                maxIndex = Math.Max(index, maxIndex);
            }

            Assert.Equal(combination.Length, maxIndex - minIndex);
            
            for (int i = 0; i < combination.Length; i++)
                Assert.Contains((int) ordering[minIndex + i].Offset, combination);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void SequenceShouldStartWithEntrypointNode(long entrypoint)
        {
            var cfg = GenerateGraph(2);
            cfg.Nodes[0].ConnectWith(cfg.Nodes[1], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[1].ConnectWith(cfg.Nodes[0], ControlFlowEdgeType.Unconditional);

            var sorting = GetSorting(cfg);
            Assert.Equal(entrypoint, sorting[0].Offset);
        }

        [Fact]
        public void FallThroughEdgesShouldStickTogether()
        {
            var cfg = GenerateGraph(8);
            cfg.Nodes[0].ConnectWith(cfg.Nodes[1], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[1].ConnectWith(cfg.Nodes[6], ControlFlowEdgeType.Unconditional);
            
            cfg.Nodes[6].ConnectWith(cfg.Nodes[2], ControlFlowEdgeType.Conditional);
            cfg.Nodes[6].ConnectWith(cfg.Nodes[7], ControlFlowEdgeType.FallThrough);
            
            cfg.Nodes[2].ConnectWith(cfg.Nodes[3], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[2].ConnectWith(cfg.Nodes[4], ControlFlowEdgeType.Conditional);
            
            cfg.Nodes[3].ConnectWith(cfg.Nodes[5], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[4].ConnectWith(cfg.Nodes[5], ControlFlowEdgeType.FallThrough);
            
            cfg.Nodes[5].ConnectWith(cfg.Nodes[6], ControlFlowEdgeType.FallThrough);

            var sorting = GetSorting(cfg);
            AssertHasSubSequence(sorting, 0, 1);
            AssertHasSubSequence(sorting, 2, 3);
            AssertHasSubSequence(sorting, 4, 5, 6, 7);
        }

        [Fact]
        public void MultipleIncomingFallThroughEdgesShouldThrow()
        {
            var cfg = GenerateGraph(3);
            cfg.Nodes[0].ConnectWith(cfg.Nodes[2], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[1].ConnectWith(cfg.Nodes[2], ControlFlowEdgeType.FallThrough);

            Assert.Throws<BlockOrderingException>(() => GetSorting(cfg));
        }

        [Fact]
        public void NodesInBasicRegionShouldStickTogether()
        {
            var cfg = GenerateGraph(4);
            
            cfg.Nodes[0].ConnectWith(cfg.Nodes[3], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[3].ConnectWith(cfg.Nodes[2], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[2].ConnectWith(cfg.Nodes[1], ControlFlowEdgeType.FallThrough);

            var region = new BasicControlFlowRegion<int>();
            cfg.Regions.Add(region);
            
            region.Nodes.Add(cfg.Nodes[3]);
            region.Nodes.Add(cfg.Nodes[2]);
                
            var sorting = GetSorting(cfg);
            AssertHasSubSequence(sorting, 2, 1);
            AssertHasCombination(sorting, 2, 3);
        }
    }
}