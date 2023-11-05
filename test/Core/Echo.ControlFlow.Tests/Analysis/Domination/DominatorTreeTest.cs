using System.IO;
using Echo.ControlFlow.Analysis.Domination;
using Echo.ControlFlow.Regions;
using Echo.Platforms.DummyPlatform;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Analysis.Domination
{
    public class DominatorTreeTest
    { 
        [Fact]
        public void SingleNode()
        {
            var graph = TestGraphs.CreateSingularGraph();

            var dominatorTree = DominatorTree<DummyInstruction>.FromGraph(graph);
            Assert.Equal(graph.EntryPoint, dominatorTree.Root.OriginalNode);
            Assert.True(dominatorTree.Dominates(graph.EntryPoint, graph.EntryPoint));
        }

        [Fact]
        public void Path()
        {
            // Artificially construct a path of four nodes in sequential order. 
            var graph = TestGraphs.CreatePath();
            var n1 = graph.GetNodeByOffset(0);
            var n2 = graph.GetNodeByOffset(1);
            var n3 = graph.GetNodeByOffset(2);
            var n4 = graph.GetNodeByOffset(3);

            var dominatorTree = DominatorTree<DummyInstruction>.FromGraph(graph);
            Assert.Equal(graph.EntryPoint, dominatorTree.Root.OriginalNode);
            
            Assert.True(dominatorTree.Dominates(n1, n1));
            Assert.True(dominatorTree.Dominates(n1, n2));
            Assert.True(dominatorTree.Dominates(n1, n3));
            Assert.True(dominatorTree.Dominates(n1, n4));
            
            Assert.False(dominatorTree.Dominates(n2, n1));
            Assert.True(dominatorTree.Dominates(n2, n2));
            Assert.True(dominatorTree.Dominates(n2, n3));
            Assert.True(dominatorTree.Dominates(n2, n4));
            
            Assert.False(dominatorTree.Dominates(n3, n1));
            Assert.False(dominatorTree.Dominates(n3, n2));
            Assert.True(dominatorTree.Dominates(n3, n3));
            Assert.True(dominatorTree.Dominates(n3, n4));
            
            Assert.False(dominatorTree.Dominates(n4, n1));
            Assert.False(dominatorTree.Dominates(n4, n2));
            Assert.False(dominatorTree.Dominates(n4, n3));
            Assert.True(dominatorTree.Dominates(n4, n4));
        }
        
        [Fact]
        public void If()
        {
            // Artificially construct an if construct.
            var graph = TestGraphs.CreateIfElse();
            var n1 = graph.GetNodeByOffset(0);
            var n2 = graph.GetNodeByOffset(2);
            var n3 = graph.GetNodeByOffset(3);
            var n4 = graph.GetNodeByOffset(4);
            
            var dominatorTree = DominatorTree<DummyInstruction>.FromGraph(graph);
            Assert.Equal(graph.EntryPoint, dominatorTree.Root.OriginalNode);
            
            Assert.True(dominatorTree.Dominates(n1, n1));
            Assert.True(dominatorTree.Dominates(n1, n2));
            Assert.True(dominatorTree.Dominates(n1, n3));
            Assert.True(dominatorTree.Dominates(n1, n4));
            
            Assert.False(dominatorTree.Dominates(n2, n1));
            Assert.True(dominatorTree.Dominates(n2, n2));
            Assert.False(dominatorTree.Dominates(n2, n3));
            Assert.False(dominatorTree.Dominates(n2, n4));
            
            Assert.False(dominatorTree.Dominates(n3, n1));
            Assert.False(dominatorTree.Dominates(n3, n2));
            Assert.True(dominatorTree.Dominates(n3, n3));
            Assert.False(dominatorTree.Dominates(n3, n4));
            
            Assert.False(dominatorTree.Dominates(n4, n1));
            Assert.False(dominatorTree.Dominates(n4, n2));
            Assert.False(dominatorTree.Dominates(n4, n3));
            Assert.True(dominatorTree.Dominates(n4, n4));
        }

        [Fact]
        public void Loop()
        {
            // Artificially construct a looping construct.
            var graph = TestGraphs.CreateLoop();
            var n1 = graph.GetNodeByOffset(0);
            var n2 = graph.GetNodeByOffset(1);
            var n3 = graph.GetNodeByOffset(2);
            var n4 = graph.GetNodeByOffset(4);
            
            var dominatorTree = DominatorTree<DummyInstruction>.FromGraph(graph);
            Assert.Equal(graph.EntryPoint, dominatorTree.Root.OriginalNode);
            
            Assert.True(dominatorTree.Dominates(n1, n1));
            Assert.True(dominatorTree.Dominates(n1, n2));
            Assert.True(dominatorTree.Dominates(n1, n3));
            Assert.True(dominatorTree.Dominates(n1, n4));
            
            Assert.False(dominatorTree.Dominates(n2, n1));
            Assert.True(dominatorTree.Dominates(n2, n2));
            Assert.False(dominatorTree.Dominates(n2, n3));
            Assert.False(dominatorTree.Dominates(n2, n4));
            
            Assert.False(dominatorTree.Dominates(n3, n1));
            Assert.True(dominatorTree.Dominates(n3, n2));
            Assert.True(dominatorTree.Dominates(n3, n3));
            Assert.True(dominatorTree.Dominates(n3, n4));
            
            Assert.False(dominatorTree.Dominates(n4, n1));
            Assert.False(dominatorTree.Dominates(n4, n2));
            Assert.False(dominatorTree.Dominates(n4, n3));
            Assert.True(dominatorTree.Dominates(n4, n4));
        }

        [Fact]
        public void ExceptionHandler()
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            for (int i = 0; i < 7; i++)
                cfg.Nodes.Add(new ControlFlowNode<int>(i));

            cfg.EntryPoint = cfg.Nodes[0];

            cfg.Nodes[0].ConnectWith(cfg.Nodes[1]);
            cfg.Nodes[1].ConnectWith(cfg.Nodes[2], ControlFlowEdgeType.Conditional);
            cfg.Nodes[1].ConnectWith(cfg.Nodes[3], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[2].ConnectWith(cfg.Nodes[4], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[3].ConnectWith(cfg.Nodes[4], ControlFlowEdgeType.FallThrough);
            cfg.Nodes[4].ConnectWith(cfg.Nodes[6], ControlFlowEdgeType.Unconditional);
            cfg.Nodes[5].ConnectWith(cfg.Nodes[6], ControlFlowEdgeType.Unconditional);

            var ehRegion = new ExceptionHandlerRegion<int>();
            cfg.Regions.Add(ehRegion);

            ehRegion.ProtectedRegion.EntryPoint = cfg.Nodes[1];
            ehRegion.ProtectedRegion.Nodes.AddRange(new[]
            {
                cfg.Nodes[1],
                cfg.Nodes[2],
                cfg.Nodes[3],
                cfg.Nodes[4],
            });
            
            var handler = new HandlerRegion<int>();
            ehRegion.Handlers.Add(handler);
            handler.Contents.Nodes.Add(cfg.Nodes[5]);
            handler.Contents.EntryPoint = cfg.Nodes[5];

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.True(tree.Dominates(cfg.Nodes[1], cfg.Nodes[6]));
            Assert.False(tree.Dominates(cfg.Nodes[4], cfg.Nodes[6]));
            Assert.False(tree.Dominates(cfg.Nodes[5], cfg.Nodes[6]));
        }
    }
}