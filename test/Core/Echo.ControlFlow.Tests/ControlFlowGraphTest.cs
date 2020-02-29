using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Platforms.DummyPlatform;
using Xunit;

namespace Echo.ControlFlow.Tests
{
    public class GraphTest
    {
        [Fact]
        public void Empty()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            Assert.Empty(graph.Nodes);
            Assert.Empty(graph.GetEdges());
        }

        [Fact]
        public void SingleNode()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var node = new ControlFlowNode<int>(0, 0);
            graph.Nodes.Add(node);

            Assert.Single(graph.Nodes);
            Assert.Equal(graph, node.ParentGraph);
        }

        [Fact]
        public void AddMultipleNodes()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = new HashSet<ControlFlowNode<int>>
            {
                new ControlFlowNode<int>(0, 0),
                new ControlFlowNode<int>(1, 1),
                new ControlFlowNode<int>(2, 2),
                new ControlFlowNode<int>(3, 3),
            };

            graph.Nodes.AddRange(nodes);
            Assert.Equal(nodes, new HashSet<ControlFlowNode<int>>(graph.Nodes));
        }

        [Fact]
        public void Entrypoint()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var node = new ControlFlowNode<int>(0, 0);
            graph.Nodes.Add(node);

            graph.Entrypoint = node;
            
            Assert.Single(graph.Nodes);
            Assert.Equal(node, graph.Entrypoint);
        }

        [Fact]
        public void AddEdge()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            var n1 = new ControlFlowNode<int>(0, 0);
            var n2 = new ControlFlowNode<int>(1, 1);

            graph.Nodes.AddRange(new[] {n1, n2});
            n1.ConnectWith(n2);

            Assert.Single(graph.GetEdges());
        }

        [Fact]
        public void AddMultipleEdges()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            var n1 = new ControlFlowNode<int>(0, 0);
            var n2 = new ControlFlowNode<int>(1, 1);
            var n3 = new ControlFlowNode<int>(2, 2);

            graph.Nodes.AddRange(new[] {n1, n2, n3});

            var edges = new HashSet<ControlFlowEdge<int>>
            {
                n1.ConnectWith(n2, ControlFlowEdgeType.Conditional),
                n1.ConnectWith(n3),
                n2.ConnectWith(n3),
            };

            Assert.Equal(edges, new HashSet<ControlFlowEdge<int>>(graph.GetEdges()));
        }

        [Fact]
        public void AddFromAnotherGraph()
        {
            var graph1 = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var graph2 = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            var n1 = new ControlFlowNode<int>(0, 0);
            var n2 = new ControlFlowNode<int>(1, 1);

            graph1.Nodes.Add(n1);
            graph2.Nodes.Add(n2);
            
            Assert.Throws<ArgumentException>(() => graph1.Nodes.Add(n2));
        }
        
        [Fact]
        public void AddManyFromAnotherGraph()
        {
            var graph1 = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var graph2 = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            var n1a = new ControlFlowNode<int>(0, 0);
            var n1b = new ControlFlowNode<int>(1, 1);
            
            var n2 = new ControlFlowNode<int>(2, 2);
            
            var n3a = new ControlFlowNode<int>(3, 3);
            var n3b = new ControlFlowNode<int>(4, 4);

            graph1.Nodes.AddRange(new[] {n1a, n1b});
            graph2.Nodes.Add(n2);

            Assert.Throws<ArgumentException>(() => graph1.Nodes.AddRange(new[]
            {
                n3a, n2, n3b
            }));
        }

        [Fact]
        public void RemoveNodeShouldRemoveIncomingEdges()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var n1 = new ControlFlowNode<int>(1);
            var n2 = new ControlFlowNode<int>(2);
            
            graph.Nodes.Add(n1);
            graph.Nodes.Add(n2);

            n1.ConnectWith(n2);

            Assert.Single(n1.GetOutgoingEdges());
            Assert.Single(n2.GetIncomingEdges());

            graph.Nodes.Remove(n2);

            Assert.Empty(n1.GetOutgoingEdges());
            Assert.Empty(n2.GetIncomingEdges());
        }

        [Fact]
        public void RemoveNodeShouldRemoveOutgoingEdges()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var n1 = new ControlFlowNode<int>(1);
            var n2 = new ControlFlowNode<int>(2);
            
            graph.Nodes.Add(n1);
            graph.Nodes.Add(n2);

            n1.ConnectWith(n2);

            Assert.Single(n1.GetOutgoingEdges());
            Assert.Single(n2.GetIncomingEdges());

            graph.Nodes.Remove(n1);

            Assert.Empty(n1.GetOutgoingEdges());
            Assert.Empty(n2.GetIncomingEdges());
        }

        [Fact]
        public void MultipleConditionalEdgesToSameNodeIsAllowed()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            var n1 = new ControlFlowNode<int>(1);
            var n2 = new ControlFlowNode<int>(2);
            var n3 = new ControlFlowNode<int>(3);

            graph.Nodes.AddRange(new[]
            {
                n1, n2, n3
            });

            n1.ConnectWith(n2);
            n1.ConnectWith(n3, ControlFlowEdgeType.Conditional);
            n1.ConnectWith(n3, ControlFlowEdgeType.Conditional);
            n1.ConnectWith(n3, ControlFlowEdgeType.Conditional);
            n1.ConnectWith(n3, ControlFlowEdgeType.Conditional);
            
            Assert.Same(n2, n1.FallThroughNeighbour);
            Assert.Equal(new[]
            {
                n3, n3, n3, n3
            }, n1.ConditionalEdges.Select(e => e.Target));
        }

        [Fact]
        public void UpdateOffsetsWithNoChangeShouldReuseNodeInstances()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            var n1 = new ControlFlowNode<int>(1, 1);
            var n2 = new ControlFlowNode<int>(2, 2);
            var n3 = new ControlFlowNode<int>(3, 3);

            graph.Nodes.AddRange(new[]
            {
                n1, n2, n3
            });
            
            graph.Nodes.UpdateOffsets();
            
            Assert.Same(n1, graph.Nodes[1]);
            Assert.Same(n2, graph.Nodes[2]);
            Assert.Same(n3, graph.Nodes[3]);
        }

        [Fact]
        public void UpdateOffsetsWithChangeShouldReuseNodeInstances()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            var n1 = new ControlFlowNode<int>(1, 1);
            var n2 = new ControlFlowNode<int>(2, 2);
            var n3 = new ControlFlowNode<int>(3, 3);

            graph.Nodes.AddRange(new[]
            {
                n1, n2, n3
            });

            n1.Contents.Instructions[0] = 4;
            n2.Contents.Instructions[0] = 5;
            n3.Contents.Instructions[0] = 6;
            
            graph.Nodes.UpdateOffsets();
            
            Assert.Same(n1, graph.Nodes[4]);
            Assert.Same(n2, graph.Nodes[5]);
            Assert.Same(n3, graph.Nodes[6]);
        }

        [Fact]
        public void UpdateOffsetsShouldUpdateNodeOffset()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var n1 = new ControlFlowNode<int>(1, 1);
            graph.Nodes.Add(n1);
            
            n1.Contents.Instructions[0] = 5;
            graph.Nodes.UpdateOffsets();
            
            Assert.Equal(5, n1.Offset);
            Assert.Equal(5, n1.Contents.Offset);
        }

        [Fact]
        public void UpdateOffsetsOnEmptyBasicBlocksShouldThrowAndDiscard()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var n1 = new ControlFlowNode<int>(1, 1);
            var n2 = new ControlFlowNode<int>(2);
            graph.Nodes.AddRange(new[]
            {
                n1,
                n2
            });

            Assert.Throws<InvalidOperationException>(() => graph.Nodes.UpdateOffsets());

            Assert.Equal(1, n1.Offset);
            Assert.Equal(1, n1.Contents.Offset);
            Assert.Equal(2, n2.Offset);
        }

        [Fact]
        public void UpdateOffsetsWithDuplicatedOffsetsShouldThrowAndDiscard()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var n1 = new ControlFlowNode<int>(1, 1);
            var n2 = new ControlFlowNode<int>(2, 2);
            var n3 = new ControlFlowNode<int>(3, 3);
            graph.Nodes.AddRange(new[]
            {
                n1,
                n2,
                n3
            });

            n2.Contents.Instructions[0] = 4;
            n3.Contents.Instructions[0] = 4;
            
            Assert.Throws<InvalidOperationException>(() => graph.Nodes.UpdateOffsets());
            
            Assert.Equal(1, n1.Offset);
            Assert.Equal(1, n1.Contents.Offset);
            Assert.Equal(2, n2.Offset);
            Assert.Equal(2, n2.Contents.Offset);
            Assert.Equal(3, n3.Offset);
            Assert.Equal(3, n3.Contents.Offset);
        }
    }
}