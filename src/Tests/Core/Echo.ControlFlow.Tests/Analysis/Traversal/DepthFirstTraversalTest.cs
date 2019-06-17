using Echo.ControlFlow.Analysis.Traversal;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Analysis.Traversal
{
    public class DepthFirstTraversalTest
    {
        [Fact]
        public void SingleNode()
        {
            var graph = new Graph<DummyInstruction>();

            var n1 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Ret(0) }
            };
            
            graph.Nodes.Add(n1);
            graph.Entrypoint = n1;
            
            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(graph.Entrypoint);

            Assert.Single(recorder.GetTraversal());
            Assert.Equal(0, recorder.GetIndex(n1));
        }

        [Fact]
        public void Path()
        {
            // Artificially construct a path of four nodes in sequential order. 
            var graph = new Graph<DummyInstruction>();

            var n1 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Op(0, 0, 0) }
            };

            var n2 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Op(1,0, 0) }
            };

            var n3 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Op(2, 0, 0) }
            };

            var n4 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Ret(3) }
            };

            graph.Nodes.AddRange(new[] {n1, n2, n3, n4});
            graph.Entrypoint = n1;
            
            n1.ConnectWith(n2);
            n2.ConnectWith(n3);
            n3.ConnectWith(n4);

            // Record a depth first traversal.
            var traversal = new DepthFirstTraversal();
            var recorder = new TraversalOrderRecorder(traversal);
            traversal.Run(graph.Entrypoint);

            // Traversal should exactly be the path.
            Assert.Equal(new[] {n1, n2, n3, n4}, recorder.GetTraversal());
        }
        
        [Fact]
        public void If()
        {
            // Artificially construct an if construct.
            var graph = new Graph<DummyInstruction>();

            var n1 = new Node<DummyInstruction>
            {
                Instructions =
                {
                    DummyInstruction.Op(0, 0, 1),
                    DummyInstruction.JmpCond(1, 3),
                }
            };

            var n2 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Jmp(2, 4) }
            };

            var n3 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Op(3, 0, 0) }
            };

            var n4 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Ret(4) }
            };

            graph.Nodes.AddRange(new[] {n1, n2, n3, n4});
            graph.Entrypoint = n1;
            
            n1.ConnectWith(n2);
            n1.ConnectWith(n3, EdgeType.Conditional);
            n2.ConnectWith(n4);
            n3.ConnectWith(n4);

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
            var graph = new Graph<DummyInstruction>();

            var n1 = new Node<DummyInstruction>
            {
                Instructions =
                {
                    DummyInstruction.Jmp(0, 2),
                }
            };

            var n2 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Jmp(1, 4) }
            };

            var n3 = new Node<DummyInstruction>
            {
                Instructions =
                {
                    DummyInstruction.Op(0, 0, 1),
                    DummyInstruction.JmpCond(3, 1)
                }
            };

            var n4 = new Node<DummyInstruction>
            {
                Instructions = { DummyInstruction.Ret(4) }
            };

            graph.Nodes.AddRange(new[] {n1, n2, n3, n4});
            graph.Entrypoint = n1;
            
            n1.ConnectWith(n3);
            n3.ConnectWith(n2, EdgeType.Conditional);
            n3.ConnectWith(n4);
            n2.ConnectWith(n3);
            
            
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