using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Graphing.Serialization.Dot;
using Echo.Platforms.DummyPlatform;
using Xunit;

namespace Echo.ControlFlow.Tests.Serialization.Blocks
{
    public class BlockSorterTest
    {
        private static ControlFlowGraph<int> GenerateGraph(int nodeCount)
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);
            for (int i = 0; i < nodeCount; i++)
                cfg.Nodes.Add(new ControlFlowNode<int>(i, i));
            cfg.Entrypoint = cfg.Nodes[0];
            return cfg;
        }

        private static void AssertHasSubSequence(ControlFlowNode<int>[] ordering, params int[] subSequence)
        {
            var cfg = ordering[0].ParentGraph;
            int index = Array.IndexOf(ordering, cfg.Nodes[subSequence[0]]);
            Assert.NotEqual(-1, index);

            for (int i = 0; i < subSequence.Length; i++)
                Assert.Equal(cfg.Nodes[subSequence[i]], ordering[i + index]);
        }

        private static void AssertHasCluster(ControlFlowNode<int>[] ordering, params int[] cluster)
        {
            var expected = new HashSet<ControlFlowNode<int>>();
            var cfg = ordering[0].ParentGraph;

            int minIndex = int.MaxValue;
            int maxIndex = int.MinValue;

            foreach (int id in cluster)
            {
                var node = cfg.Nodes[id];
                expected.Add(node);

                int index = Array.IndexOf(ordering, node);
                Assert.NotEqual(-1, index);
                minIndex = Math.Min(index, minIndex);
                maxIndex = Math.Max(index, maxIndex);
            }

            Assert.Equal(cluster.Length, maxIndex - minIndex + 1);

            var range = ordering[minIndex..(maxIndex + 1)];
            Assert.Equal(expected, new HashSet<ControlFlowNode<int>>(range));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void SequenceShouldStartWithEntrypointNode(long entrypoint)
        {
            var cfg = GenerateGraph(2);
            cfg.Entrypoint = cfg.Nodes[entrypoint];
            cfg.Nodes[0].ConnectWith(cfg.Nodes[1], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[1].ConnectWith(cfg.Nodes[0], ControlFlowEdgeType.Unconditional);

            var sorting = cfg
                .SortNodes()
                .ToArray();

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

            var sorting = cfg
                .SortNodes()
                .ToArray();

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

            Assert.Throws<BlockOrderingException>(() => cfg.SortNodes());
        }

        [Theory]
        [InlineData(new long[] {0, 1, 2, 3})]
        [InlineData(new long[] {3, 2, 1, 0})]
        [InlineData(new long[] {2, 3, 0, 1})]
        public void PreferExitPointsLastInDoLoop(long[] indices)
        {
            var cfg = GenerateGraph(4);

            cfg.Nodes[indices[0]].ConnectWith(cfg.Nodes[indices[1]], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[indices[1]].ConnectWith(cfg.Nodes[indices[2]], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[indices[2]].ConnectWith(cfg.Nodes[indices[3]], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[indices[2]].ConnectWith(cfg.Nodes[indices[1]], ControlFlowEdgeType.Conditional);
            cfg.Entrypoint = cfg.Nodes[indices[0]];

            var sorting = cfg
                .SortNodes()
                .ToArray();

            Assert.Equal(new[]
            {
                indices[0], indices[1], indices[2], indices[3]
            }, sorting.Select(n => n.Offset));
        }

        [Theory]
        [InlineData(new long[] {0, 1, 2, 3})]
        [InlineData(new long[] {3, 2, 1, 0})]
        [InlineData(new long[] {2, 3, 0, 1})]
        public void PreferExitPointsLastInWhileLoop(long[] indices)
        {
            var cfg = GenerateGraph(4);

            cfg.Nodes[indices[0]].ConnectWith(cfg.Nodes[indices[2]], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[indices[1]].ConnectWith(cfg.Nodes[indices[2]], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[indices[2]].ConnectWith(cfg.Nodes[indices[3]], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[indices[2]].ConnectWith(cfg.Nodes[indices[1]], ControlFlowEdgeType.Conditional);

            cfg.Entrypoint = cfg.Nodes[indices[0]];

            var sorting = cfg
                .SortNodes()
                .ToArray();

            Assert.Equal(new[]
            {
                indices[0], indices[1], indices[2], indices[3]
            }, sorting.Select(n => n.Offset));
        }

        [Theory]
        [InlineData(new[] {0, 1, 2, 3, 4, 5, 6})]
        [InlineData(new[] {6, 5, 4, 3, 2, 1, 0})]
        [InlineData(new[] {1, 5, 3, 6, 4, 0, 2})]
        public void NodesInExceptionHandlerBlocksShouldStickTogether(int[] indices)
        {
            var cfg = GenerateGraph(7);

            // pre
            cfg.Nodes[indices[0]].ConnectWith(cfg.Nodes[indices[1]], ControlFlowEdgeType.FallThrough);

            // protected region.
            cfg.Nodes[indices[1]].ConnectWith(cfg.Nodes[indices[2]], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[indices[1]].ConnectWith(cfg.Nodes[indices[3]], ControlFlowEdgeType.Conditional);
            cfg.Nodes[indices[2]].ConnectWith(cfg.Nodes[indices[4]], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[indices[3]].ConnectWith(cfg.Nodes[indices[4]], ControlFlowEdgeType.FallThrough);

            // handler region.
            cfg.Nodes[indices[5]].ConnectWith(cfg.Nodes[indices[6]], ControlFlowEdgeType.Unconditional);

            // post
            cfg.Nodes[indices[4]].ConnectWith(cfg.Nodes[indices[6]], ControlFlowEdgeType.Unconditional);

            cfg.Entrypoint = cfg.Nodes[indices[0]];

            // Set up regions.
            var ehRegion = new ExceptionHandlerRegion<int>();
            cfg.Regions.Add(ehRegion);
            ehRegion.ProtectedRegion.Nodes.AddRange(new[]
            {
                cfg.Nodes[indices[1]], cfg.Nodes[indices[2]], cfg.Nodes[indices[3]], cfg.Nodes[indices[4]]
            });
            var handlerRegion = new HandlerRegion<int>();
            ehRegion.Handlers.Add(handlerRegion);
            handlerRegion.Contents.Nodes.Add(cfg.Nodes[indices[5]]);
            handlerRegion.Contents.Entrypoint = cfg.Nodes[indices[5]];

            // Sort
            var sorting = cfg
                .SortNodes()
                .ToArray();

            // Validate.
            AssertHasSubSequence(sorting, indices[0], indices[1], indices[2]);
            AssertHasSubSequence(sorting, indices[3], indices[4]);
            AssertHasCluster(sorting, indices[1], indices[2], indices[3], indices[4]);
            Assert.True(Array.IndexOf(sorting, cfg.Nodes[indices[1]]) < Array.IndexOf(sorting, cfg.Nodes[indices[5]]),
                "Handler region was ordered before protected region.");
        }

        [Theory]
        [InlineData(new[] {0, 1, 2, 3})]
        [InlineData(new[] {3, 2, 1, 0})]
        [InlineData(new[] {2, 3, 0, 1})]
        public void HandlerRegionWithNoExitShouldBeOrderedBeforeNormalExit(int[] indices)
        {
            var cfg = GenerateGraph(4);
            cfg.Entrypoint = cfg.Nodes[indices[0]];

            cfg.Nodes[indices[0]].ConnectWith(cfg.Nodes[indices[1]], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[indices[1]].ConnectWith(cfg.Nodes[indices[3]], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[indices[2]].ConnectWith(cfg.Nodes[indices[2]], ControlFlowEdgeType.Unconditional);

            // Set up regions.
            var ehRegion = new ExceptionHandlerRegion<int>();
            cfg.Regions.Add(ehRegion);
            ehRegion.ProtectedRegion.Nodes.Add(cfg.Nodes[indices[1]]);
            var handlerRegion = new HandlerRegion<int>();
            ehRegion.Handlers.Add(handlerRegion);
            handlerRegion.Contents.Nodes.Add(cfg.Nodes[indices[2]]);
            handlerRegion.Contents.Entrypoint = cfg.Nodes[indices[2]];

            // Sort
            var sorting = cfg
                .SortNodes()
                .ToArray();
            
            Assert.True(Array.IndexOf(sorting, cfg.Nodes[indices[2]]) < Array.IndexOf(sorting, cfg.Nodes[indices[3]]),
                "Handler region was ordered after normal exit.");
        }

        [Theory]
        [InlineData(new[] {0, 1, 2, 3, 4, 5})]
        [InlineData(new[] {5, 4, 3, 2, 1, 0})]
        [InlineData(new[] {2, 3, 0, 4, 5, 1})]
        public void PrologueAndEpilogueRegionsShouldHaveCorrectPrecedence(int[] indices)
        {
            var cfg = GenerateGraph(6);
            cfg.Entrypoint = cfg.Nodes[indices[0]];

            var eh = new ExceptionHandlerRegion<int>();
            cfg.Regions.Add(eh);
            
            eh.ProtectedRegion.Nodes.Add(cfg.Nodes[indices[1]]);
            eh.ProtectedRegion.Entrypoint = cfg.Nodes[indices[1]];
            
            var handler = new HandlerRegion<int>();
            eh.Handlers.Add(handler);
            
            handler.Prologue = new ScopeRegion<int>();
            handler.Prologue.Nodes.Add(cfg.Nodes[indices[2]]);
            handler.Prologue.Entrypoint = cfg.Nodes[indices[2]];
            
            handler.Contents.Nodes.Add(cfg.Nodes[indices[3]]);
            handler.Contents.Entrypoint = cfg.Nodes[indices[3]];
            
            handler.Epilogue = new ScopeRegion<int>();
            handler.Epilogue.Nodes.Add(cfg.Nodes[indices[4]]);
            handler.Epilogue.Entrypoint = cfg.Nodes[indices[4]];
        
            cfg.Nodes[indices[0]].ConnectWith(cfg.Nodes[indices[1]]);
            cfg.Nodes[indices[1]].ConnectWith(cfg.Nodes[indices[5]], ControlFlowEdgeType.Unconditional);
        
            var sorted = cfg.SortNodes();
            Assert.Equal(new[]
            {
                cfg.Nodes[indices[0]], // start
                cfg.Nodes[indices[1]], // protected
                cfg.Nodes[indices[2]], // prologue
                cfg.Nodes[indices[3]], // handler
                cfg.Nodes[indices[4]], // epilogue
                cfg.Nodes[indices[5]], // exit
            }, sorted);
        }

        [Theory]
        [InlineData(new[] {0, 1, 2, 3, 4, 5})]
        [InlineData(new[] {5, 4, 3, 2, 1, 0})]
        [InlineData(new[] {2, 4, 3, 0, 5, 1})]
        public void HandlerWithNoLeaveBranch(int[] indices)
        {
            var cfg = GenerateGraph(6);
            cfg.Entrypoint = cfg.Nodes[indices[0]];
            
            var eh = new ExceptionHandlerRegion<int>();
            cfg.Regions.Add(eh);

            eh.ProtectedRegion.Nodes.AddRange(new[]
            {
                cfg.Nodes[indices[1]],
                cfg.Nodes[indices[2]],
                cfg.Nodes[indices[3]]
            });
            eh.ProtectedRegion.Entrypoint = cfg.Nodes[indices[1]];
            
            var handler = new HandlerRegion<int>();
            eh.Handlers.Add(handler);
            handler.Contents.Nodes.Add(cfg.Nodes[indices[4]]);
            handler.Contents.Entrypoint = cfg.Nodes[indices[4]];

            cfg.Nodes[indices[0]].ConnectWith(cfg.Nodes[indices[1]]);
            cfg.Nodes[indices[1]].ConnectWith(cfg.Nodes[indices[2]]);
            cfg.Nodes[indices[2]].ConnectWith(cfg.Nodes[indices[3]]);
            cfg.Nodes[indices[2]].ConnectWith(cfg.Nodes[indices[1]], ControlFlowEdgeType.Conditional);
            cfg.Nodes[indices[3]].ConnectWith(cfg.Nodes[indices[5]], ControlFlowEdgeType.Unconditional);
            
            var sorted = cfg.SortNodes();
            Assert.Equal(new[]
            {
                cfg.Nodes[indices[0]], 
                cfg.Nodes[indices[1]], // protected
                cfg.Nodes[indices[2]], // protected
                cfg.Nodes[indices[3]], // protected
                cfg.Nodes[indices[4]], // handler
                cfg.Nodes[indices[5]],
            }, sorted);
        }
    }
}