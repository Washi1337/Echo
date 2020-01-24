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
            var graph = new Graph<int>();
            
            Assert.Empty(graph.Nodes);
            Assert.Empty(graph.GetEdges());
        }

        [Fact]
        public void SingleNode()
        {
            var graph = new Graph<int>();

            var node = new Node<int>(0, 0);
            graph.Nodes.Add(node);

            Assert.Single(graph.Nodes);
            Assert.Equal(graph, node.ParentGraph);
        }

        [Fact]
        public void AddMultipleNodes()
        {
            var graph = new Graph<int>();

            var nodes = new HashSet<Node<int>>
            {
                new Node<int>(0, 0),
                new Node<int>(1, 1),
                new Node<int>(2, 2),
                new Node<int>(3, 3),
            };

            graph.Nodes.AddRange(nodes);
            Assert.Equal(nodes, new HashSet<Node<int>>(graph.Nodes));
        }

        [Fact]
        public void Entrypoint()
        {
            var graph = new Graph<int>();

            var node = new Node<int>(0, 0);
            graph.Nodes.Add(node);

            graph.Entrypoint = node;
            
            Assert.Single(graph.Nodes);
            Assert.Equal(node, graph.Entrypoint);
        }

        [Fact]
        public void AddEdge()
        {
            var graph = new Graph<int>();
            
            var n1 = new Node<int>(0, 0);
            var n2 = new Node<int>(1, 1);

            graph.Nodes.AddRange(new[] {n1, n2});
            n1.ConnectWith(n2);

            Assert.Single(graph.GetEdges());
        }

        [Fact]
        public void AddMultipleEdges()
        {
            var graph = new Graph<int>();
            
            var n1 = new Node<int>(0, 0);
            var n2 = new Node<int>(1, 1);
            var n3 = new Node<int>(2, 2);

            graph.Nodes.AddRange(new[] {n1, n2, n3});

            var edges = new HashSet<Edge<int>>
            {
                n1.ConnectWith(n2, EdgeType.Conditional),
                n1.ConnectWith(n3),
                n2.ConnectWith(n3),
            };

            Assert.Equal(edges, new HashSet<Edge<int>>(graph.GetEdges()));
        }

        [Fact]
        public void AddFromAnotherGraph()
        {
            var graph1 = new Graph<int>();
            var graph2 = new Graph<int>();
            
            var n1 = new Node<int>(0, 0);
            var n2 = new Node<int>(1, 1);

            graph1.Nodes.Add(n1);
            graph2.Nodes.Add(n2);
            
            Assert.Throws<ArgumentException>(() => graph1.Nodes.Add(n2));
        }
        
        [Fact]
        public void AddManyFromAnotherGraph()
        {
            var graph1 = new Graph<int>();
            var graph2 = new Graph<int>();
            
            var n1a = new Node<int>(0, 0);
            var n1b = new Node<int>(1, 1);
            
            var n2 = new Node<int>(2, 2);
            
            var n3a = new Node<int>(3, 3);
            var n3b = new Node<int>(4, 4);

            graph1.Nodes.AddRange(new[] {n1a, n1b});
            graph2.Nodes.Add(n2);

            Assert.Throws<ArgumentException>(() => graph1.Nodes.AddRange(new[]
            {
                n3a, n2, n3b
            }));
        }
        
    }
}