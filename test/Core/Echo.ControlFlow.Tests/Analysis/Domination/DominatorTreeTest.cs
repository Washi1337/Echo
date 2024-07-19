using System.Linq;
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
            Assert.True(dominatorTree.Dominates(graph.EntryPoint!, graph.EntryPoint!));
        }

        [Fact]
        public void Path()
        {
            // Artificially construct a path of four nodes in sequential order. 
            var graph = TestGraphs.CreatePath();
            var n1 = graph.Nodes.GetByOffset(0)!;
            var n2 = graph.Nodes.GetByOffset(1)!;
            var n3 = graph.Nodes.GetByOffset(2)!;
            var n4 = graph.Nodes.GetByOffset(3)!;

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
            var n1 = graph.Nodes.GetByOffset(0)!;
            var n2 = graph.Nodes.GetByOffset(2)!;
            var n3 = graph.Nodes.GetByOffset(3)!;
            var n4 = graph.Nodes.GetByOffset(4)!;
            
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
            var n1 = graph.Nodes.GetByOffset(0);
            var n2 = graph.Nodes.GetByOffset(1);
            var n3 = graph.Nodes.GetByOffset(2);
            var n4 = graph.Nodes.GetByOffset(4);
            
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

            var nodes = Enumerable.Range(0, 7).Select(x => new ControlFlowNode<int>(x)).ToArray();
            cfg.Nodes.AddRange(nodes);

            cfg.EntryPoint = cfg.Nodes.GetByOffset(0);

            nodes[0].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);
            nodes[1].ConnectWith(nodes[3], ControlFlowEdgeType.FallThrough);
            nodes[2].ConnectWith(nodes[4], ControlFlowEdgeType.Unconditional);
            nodes[3].ConnectWith(nodes[4], ControlFlowEdgeType.FallThrough);
            nodes[4].ConnectWith(nodes[6], ControlFlowEdgeType.Unconditional);
            nodes[5].ConnectWith(nodes[6], ControlFlowEdgeType.Unconditional);

            var ehRegion = new ExceptionHandlerRegion<int>();
            cfg.Regions.Add(ehRegion);

            ehRegion.ProtectedRegion.EntryPoint = nodes[1];
            ehRegion.ProtectedRegion.Nodes.AddRange(new[]
            {
                nodes[1],
                nodes[2],
                nodes[3],
                nodes[4],
            });
            
            var handler = new HandlerRegion<int>();
            ehRegion.Handlers.Add(handler);
            handler.Contents.Nodes.Add(nodes[5]);
            handler.Contents.EntryPoint = nodes[5];

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.True(tree.Dominates(nodes[1], nodes[6]));
            Assert.False(tree.Dominates(nodes[4], nodes[6]));
            Assert.False(tree.Dominates(nodes[5], nodes[6]));
        }
    }
}