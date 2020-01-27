using Echo.Core.Graphing;
using Echo.Core.Graphing.Analysis.Traversal;
using Xunit;

namespace Echo.Core.Tests.Graphing.Analysis.Traversal
{
    public class DepthFirstTraversalTest
    {
        [Fact]
        public void SingleNode()
        {
            var graph = TestGraphs.CreateSingularGraph();
            var startNode = graph.GetNodeById(1);
            
            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(startNode);

            Assert.Single(recorder.GetTraversal());
            Assert.Equal(0, recorder.GetIndex(startNode));
        }

        [Fact]
        public void Path()
        {
            // Artificially construct a path of four nodes in sequential order. 
            var graph = TestGraphs.CreatePath();
            var startNode = graph.GetNodeById(1);

            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(startNode);

            // Traversal should exactly be the path.
            Assert.Equal(new[]
            {
                graph.GetNodeById(1),
                graph.GetNodeById(2),
                graph.GetNodeById(3),
                graph.GetNodeById(4),
            }, recorder.GetTraversal());
        }

        [Fact]
        public void PathReversed()
        {
            // Artificially construct a path of four nodes in sequential order. 
            var graph = TestGraphs.CreatePath();

            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal(true);
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(graph.GetNodeById(4));

            // Traversal should exactly be the path.
            Assert.Equal(new INode[]
            {
                graph.GetNodeById(4),
                graph.GetNodeById(3),
                graph.GetNodeById(2),
                graph.GetNodeById(1),
            }, recorder.GetTraversal());   
        }
        
        [Fact]
        public void If()
        {
            // Artificially construct an if construct.
            var graph = TestGraphs.CreateIfElse();

            var n1 = graph.GetNodeById(1);
            var n2 = graph.GetNodeById(2);
            var n3 = graph.GetNodeById(3);
            var n4 = graph.GetNodeById(4);

            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(n1);
            
            // Check if n1 is before any node in the traversal.
            Assert.All(graph.GetNodes(), 
                n => Assert.True(n1 == n || recorder.GetIndex(n1) < recorder.GetIndex(n)));
            
            // DFS should either pick n2 or n3. If n2, then n4 is before n3, otherwise before n2. 
            if (recorder.GetIndex(n2) < recorder.GetIndex(n3))
                Assert.True(recorder.GetIndex(n4) < recorder.GetIndex(n3));
            else
                Assert.True(recorder.GetIndex(n4) < recorder.GetIndex(n2));
        }
        
        [Fact]
        public void IfReversed()
        {
            // Artificially construct an if construct.
            var graph = TestGraphs.CreateIfElse();

            var n1 = graph.GetNodeById(1);
            var n2 = graph.GetNodeById(2);
            var n3 = graph.GetNodeById(3);
            var n4 = graph.GetNodeById(4);

            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal(true);
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(n4);
            
            // Check if n4 is before any node in the traversal.
            Assert.All(graph.GetNodes(), 
                n => Assert.True(n4 == n || recorder.GetIndex(n4) < recorder.GetIndex(n)));
            
            // DFS should either pick n2 or n3. If n2, then n1 is before n3, otherwise before n2. 
            if (recorder.GetIndex(n2) > recorder.GetIndex(n3))
                Assert.True(recorder.GetIndex(n1) > recorder.GetIndex(n3));
            else
                Assert.True(recorder.GetIndex(n1) > recorder.GetIndex(n2));
        }

        [Fact]
        public void Loop()
        {
            // Artificially construct a looping construct.
            var graph = TestGraphs.CreateLoop();
            var n1 = graph.GetNodeById(1);
            var n2 = graph.GetNodeById(2);
            var n3 = graph.GetNodeById(3);
            var n4 = graph.GetNodeById(4);
            
            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(n1);

            // Check if n1 is before any node in the traversal.
            Assert.All(graph.GetNodes(), 
                n => Assert.True(n1 == n || recorder.GetIndex(n1) < recorder.GetIndex(n)));

            Assert.True(recorder.GetIndex(n2) > recorder.GetIndex(n3));
            Assert.True(recorder.GetIndex(n4) > recorder.GetIndex(n3));
        }

        [Fact]
        public void LoopReversed()
        {
            // Artificially construct a looping construct.
            var graph = TestGraphs.CreateLoop();
            var n1 = graph.GetNodeById(1);
            var n2 = graph.GetNodeById(2);
            var n3 = graph.GetNodeById(3);
            var n4 = graph.GetNodeById(4);
            
            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal(true);
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(n4);

            // Check if n1 is before any node in the traversal.
            Assert.All(graph.GetNodes(), 
                n => Assert.True(n4 == n || recorder.GetIndex(n4) < recorder.GetIndex(n)));

            Assert.True(recorder.GetIndex(n1) > recorder.GetIndex(n3));
        }
        
    }
}