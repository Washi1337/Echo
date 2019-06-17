using Echo.ControlFlow.Analysis.Traversal;
using Xunit;

namespace Echo.ControlFlow.Tests.Analysis.Traversal
{
    public class DepthFirstTraversalTest
    {
        [Fact]
        public void SingleNode()
        {
            var graph = TestGraphs.CreateSingularGraph();
            
            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(graph.Entrypoint);

            Assert.Single(recorder.GetTraversal());
            Assert.Equal(0, recorder.GetIndex(graph.Entrypoint));
        }

        [Fact]
        public void Path()
        {
            // Artificially construct a path of four nodes in sequential order. 
            var graph = TestGraphs.CreatePath();

            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(graph.Entrypoint);

            // Traversal should exactly be the path.
            Assert.Equal(new[]
            {
                graph.GetNodeByOffset(0),
                graph.GetNodeByOffset(1),
                graph.GetNodeByOffset(2),
                graph.GetNodeByOffset(3),
            }, recorder.GetTraversal());
        }
        
        [Fact]
        public void If()
        {
            // Artificially construct an if construct.
            var graph = TestGraphs.CreateIf();

            var n1 = graph.GetNodeByOffset(0);
            var n2 = graph.GetNodeByOffset(2);
            var n3 = graph.GetNodeByOffset(3);
            var n4 = graph.GetNodeByOffset(4);

            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(graph.Entrypoint);
            
            // Check if n1 is before any node in the traversal.
            Assert.All(graph.Nodes, n => Assert.True(n1 == n || recorder.GetIndex(n1) < recorder.GetIndex(n)));
            
            // DFS should either pick n2 or n3. If n2, then n4 is before n3, otherwise before n2. 
            if (recorder.GetIndex(n2) < recorder.GetIndex(n3))
                Assert.True(recorder.GetIndex(n4) < recorder.GetIndex(n3));
            else
                Assert.True(recorder.GetIndex(n4) < recorder.GetIndex(n2));
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
            
            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(graph.Entrypoint);

            // Check if n1 is before any node in the traversal.
            Assert.All(graph.Nodes, n => Assert.True(n1 == n || recorder.GetIndex(n1) < recorder.GetIndex(n)));

            Assert.True(recorder.GetIndex(n2) > recorder.GetIndex(n3));
            Assert.True(recorder.GetIndex(n4) > recorder.GetIndex(n3));
        }
    }
}