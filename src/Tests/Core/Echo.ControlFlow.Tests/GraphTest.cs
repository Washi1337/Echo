using System;
using System.Collections.Generic;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests
{
    public class GraphTest
    {
        [Fact]
        public void Empty()
        {
            var graph = new Graph<DummyInstruction>();
            
            Assert.Empty(graph.Nodes);
            Assert.Empty(graph.GetEdges());
        }

        [Fact]
        public void SingleNode()
        {
            var graph = new Graph<DummyInstruction>();

            var node = new Node<DummyInstruction>();
            graph.Nodes.Add(node);

            Assert.Single(graph.Nodes);
            Assert.Equal(graph, node.ParentGraph);
        }

        [Fact]
        public void AddMultipleNodes()
        {
            var graph = new Graph<DummyInstruction>();

            var nodes = new HashSet<Node<DummyInstruction>>
            {
                new Node<DummyInstruction>(),
                new Node<DummyInstruction>(),
                new Node<DummyInstruction>(),
                new Node<DummyInstruction>(),
            };

            graph.Nodes.AddRange(nodes);
            Assert.Equal(nodes, new HashSet<Node<DummyInstruction>>(graph.Nodes));
        }

        [Fact]
        public void Entrypoint()
        {
            var graph = new Graph<DummyInstruction>();

            var node = new Node<DummyInstruction>();
            graph.Nodes.Add(node);

            graph.Entrypoint = node;
            
            Assert.Single(graph.Nodes);
            Assert.Equal(node, graph.Entrypoint);
        }

        [Fact]
        public void AddEdge()
        {
            var graph = new Graph<DummyInstruction>();
            
            var n1 = new Node<DummyInstruction>();
            var n2 = new Node<DummyInstruction>();

            graph.Nodes.AddRange(new[] {n1, n2});
            n1.ConnectWith(n2);

            Assert.Single(graph.GetEdges());
        }

        [Fact]
        public void AddMultipleEdges()
        {
            var graph = new Graph<DummyInstruction>();
            
            var n1 = new Node<DummyInstruction>();
            var n2 = new Node<DummyInstruction>();
            var n3 = new Node<DummyInstruction>();

            graph.Nodes.AddRange(new[] {n1, n2, n3});

            var edges = new HashSet<Edge<DummyInstruction>>
            {
                n1.ConnectWith(n2, EdgeType.Conditional),
                n1.ConnectWith(n3),
                n2.ConnectWith(n3),
            };

            Assert.Equal(edges, new HashSet<Edge<DummyInstruction>>(graph.GetEdges()));
        }

        [Fact]
        public void AddFromAnotherGraph()
        {
            var graph1 = new Graph<DummyInstruction>();
            var graph2 = new Graph<DummyInstruction>();
            
            var n1 = new Node<DummyInstruction>();
            var n2 = new Node<DummyInstruction>();

            graph1.Nodes.Add(n1);
            graph2.Nodes.Add(n2);
            
            Assert.Throws<ArgumentException>(() => graph1.Nodes.Add(n2));
        }
        
        [Fact]
        public void AddManyFromAnotherGraph()
        {
            var graph1 = new Graph<DummyInstruction>();
            var graph2 = new Graph<DummyInstruction>();
            
            var n1a = new Node<DummyInstruction>();
            var n1b = new Node<DummyInstruction>();
            
            var n2 = new Node<DummyInstruction>();
            
            var n3a = new Node<DummyInstruction>();
            var n3b = new Node<DummyInstruction>();

            graph1.Nodes.AddRange(new[] {n1a, n1b});
            graph2.Nodes.Add(n2);

            Assert.Throws<ArgumentException>(() => graph1.Nodes.AddRange(new[]
            {
                n3a, n2, n3b
            }));
        }
        
    }
}