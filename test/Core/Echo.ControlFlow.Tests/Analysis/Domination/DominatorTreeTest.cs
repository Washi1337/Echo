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
            var n1 = graph.Nodes.GetByOffset(0)!;
            var n2 = graph.Nodes.GetByOffset(1)!;
            var n3 = graph.Nodes.GetByOffset(2)!;
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

        [Fact]
        public void ParentIsNodeItWasReachedFrom()
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = Enumerable.Range(0, 8).Select(x => new ControlFlowNode<int>(x)).ToArray();
            cfg.Nodes.AddRange(nodes);
            cfg.EntryPoint = nodes[0];

            int[][] edges = [[0, 4], [1, 1], [2, 5], [4, 6], [4, 7], [5, 1], [5, 4], [6, 5], [6, 7], [7, 1], [7, 2]];
            foreach (var edge in edges)
                nodes[edge[0]].ConnectWith(nodes[edge[1]], ControlFlowEdgeType.Conditional);

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.Same(nodes[4], tree[nodes[1]].Parent!.OriginalNode);
        }

        [Fact]
        public void UnreachableNodes()
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = Enumerable.Range(0, 3).Select(x => new ControlFlowNode<int>(x)).ToArray();
            cfg.Nodes.AddRange(nodes);
            cfg.EntryPoint = nodes[0];

            nodes[0].ConnectWith(nodes[1]);
            nodes[2].ConnectWith(nodes[1]);

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.Same(nodes[0], tree[nodes[1]].Parent!.OriginalNode);
            Assert.Empty(tree.GetDominanceFrontier(nodes[0]));
            Assert.False(tree.Dominates(nodes[0], nodes[2]));
            Assert.False(tree.TryGetNode(nodes[2], out _));
        }

        [Fact]
        public void NestedFilterAndHandler()
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = Enumerable.Range(0, 5).Select(x => new ControlFlowNode<int>(x)).ToArray();
            cfg.Nodes.AddRange(nodes);
            cfg.EntryPoint = nodes[0];

            nodes[0].ConnectWith(nodes[4]);
            nodes[2].ConnectWith(nodes[4]);
            nodes[3].ConnectWith(nodes[4]);

            var outerRegion = new ExceptionHandlerRegion<int>();
            cfg.Regions.Add(outerRegion);
            outerRegion.ProtectedRegion.EntryPoint = nodes[0];

            var outerHandler = new HandlerRegion<int>();
            outerRegion.Handlers.Add(outerHandler);
            outerHandler.Contents.Nodes.Add(nodes[3]);
            outerHandler.Contents.EntryPoint = nodes[3];

            var innerRegion = new ExceptionHandlerRegion<int>();
            outerRegion.ProtectedRegion.Regions.Add(innerRegion);
            innerRegion.ProtectedRegion.EntryPoint = nodes[0];
            innerRegion.ProtectedRegion.Nodes.Add(nodes[0]);

            var innerHandler = new HandlerRegion<int>();
            innerRegion.Handlers.Add(innerHandler);
            innerHandler.Prologue = new ScopeRegion<int>();
            innerHandler.Prologue.Nodes.Add(nodes[1]);
            innerHandler.Prologue.EntryPoint = nodes[1];
            innerHandler.Contents.Nodes.Add(nodes[2]);
            innerHandler.Contents.EntryPoint = nodes[2];

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.Same(nodes[0], tree[nodes[1]].Parent!.OriginalNode);
            Assert.Same(nodes[1], tree[nodes[2]].Parent!.OriginalNode);
            Assert.Same(nodes[0], tree[nodes[3]].Parent!.OriginalNode);
            Assert.Contains(nodes[3], tree.GetDominanceFrontier(nodes[2]));
        }

        [Fact]
        public void HandlerEpilogue()
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = Enumerable.Range(0, 3).Select(x => new ControlFlowNode<int>(x)).ToArray();
            cfg.Nodes.AddRange(nodes);
            cfg.EntryPoint = nodes[0];

            var ehRegion = new ExceptionHandlerRegion<int>();
            cfg.Regions.Add(ehRegion);
            ehRegion.ProtectedRegion.Nodes.Add(nodes[0]);
            ehRegion.ProtectedRegion.EntryPoint = nodes[0];

            var handler = new HandlerRegion<int>();
            ehRegion.Handlers.Add(handler);
            handler.Contents.Nodes.Add(nodes[1]);
            handler.Contents.EntryPoint = nodes[1];
            handler.Epilogue = new ScopeRegion<int>();
            handler.Epilogue.Nodes.Add(nodes[2]);
            handler.Epilogue.EntryPoint = nodes[2];

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.Same(nodes[0], tree[nodes[1]].Parent!.OriginalNode);
            Assert.Same(nodes[1], tree[nodes[2]].Parent!.OriginalNode);
            Assert.Equal(new[] { nodes[1] }, tree[nodes[0]].GetDirectChildren().Select(c => c.OriginalNode));
            Assert.Equal(new[] { nodes[2] }, tree[nodes[1]].GetDirectChildren().Select(c => c.OriginalNode));
        }

        [Fact]
        public void ChildrenAndSuccessorFunction()
        {
            var graph = TestGraphs.CreateIfElse();
            var n1 = graph.Nodes.GetByOffset(0)!;
            var n2 = graph.Nodes.GetByOffset(2)!;
            var n3 = graph.Nodes.GetByOffset(3)!;
            var n4 = graph.Nodes.GetByOffset(4)!;

            var tree = DominatorTree<DummyInstruction>.FromGraph(graph);
            Assert.Equal(new[] { n2, n3 }, tree.Root.GetDirectChildren().Select(c => c.OriginalNode).OrderBy(n => n.Offset));
            Assert.Equal(new[] { n4 }, tree.Root.GetIndirectChildren().Select(c => c.OriginalNode));

            tree = DominatorTree<DummyInstruction>.FromGraph(n1, n => n.GetSuccessors().Where(s => s != n3));
            Assert.True(tree.Dominates(n2, n4));

            tree = DominatorTree<DummyInstruction>.FromGraph(
                n1,
                n => n == n1 ? n.GetSuccessors().Append(n4) : n.GetSuccessors());
            Assert.Contains(n4, tree.Root.GetDirectChildren().Select(c => c.OriginalNode));
        }
    }
}