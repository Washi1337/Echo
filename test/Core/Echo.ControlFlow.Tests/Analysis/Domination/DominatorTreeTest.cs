using Echo.ControlFlow.Analysis.Domination;
using Xunit;

namespace Echo.ControlFlow.Tests.Analysis.Domination
{
    public class DominatorTreeTest
    { 
        [Fact]
        public void SingleNode()
        {
            var graph = TestGraphs.CreateSingularGraph();

            var dominatorTree = DominatorTree.FromGraph(graph);
            Assert.Equal(graph.Entrypoint, dominatorTree.Root.OriginalNode);
            Assert.True(dominatorTree.Dominates(graph.Entrypoint, graph.Entrypoint));
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

            var dominatorTree = DominatorTree.FromGraph(graph);
            Assert.Equal(graph.Entrypoint, dominatorTree.Root.OriginalNode);
            
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
            
            var dominatorTree = DominatorTree.FromGraph(graph);
            Assert.Equal(graph.Entrypoint, dominatorTree.Root.OriginalNode);
            
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
            
            var dominatorTree = DominatorTree.FromGraph(graph);
            Assert.Equal(graph.Entrypoint, dominatorTree.Root.OriginalNode);
            
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
    }
}