using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests
{
    public class NodeTest
    {
        private static IList<Node<DummyInstruction>> CreateDummyGraph(int nodeCount)
        {
            var graph = new Graph<DummyInstruction>();

            var nodes = new Node<DummyInstruction>[nodeCount];
            for (int i = 0; i < nodeCount; i++)
                nodes[i] = new Node<DummyInstruction>();

            foreach (var node in nodes)
                graph.Nodes.Add(node);

            return nodes;
        }

        [Fact]
        public void ValidFallthroughEdge()
        {
            var nodes = CreateDummyGraph(2);
            nodes[0].FallThroughEdge = new Edge<DummyInstruction>(nodes[0], nodes[1]);
            
            Assert.Equal(nodes[0], nodes[0].FallThroughEdge.Source);
            Assert.Equal(nodes[1], nodes[0].FallThroughEdge.Target);
            Assert.Equal(EdgeType.FallThrough, nodes[0].FallThroughEdge.Type);
        }

        [Fact]
        public void InvalidFallthroughEdge()
        {
            var nodes = CreateDummyGraph(2);
            nodes[0].FallThroughEdge = new Edge<DummyInstruction>(nodes[0], nodes[1]);

            Assert.Throws<ArgumentException>(
                () => nodes[0].FallThroughEdge = new Edge<DummyInstruction>(nodes[1], nodes[0]));
            
            Assert.Throws<ArgumentException>(
                () => nodes[0].FallThroughEdge = new Edge<DummyInstruction>(nodes[0], nodes[1], EdgeType.Conditional));
        }
        
        [Fact]
        public void ConditionalEdge()
        { 
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConditionalEdges.Add(
                new Edge<DummyInstruction>(nodes[0], nodes[1], EdgeType.Conditional));
            Assert.Contains(edge, nodes[0].ConditionalEdges);
        }
        
        [Fact]
        public void ConditionalEdgeThroughNeighbour()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConditionalEdges.Add(nodes[1]);

            Assert.Equal(nodes[0], edge.Source);
            Assert.Equal(EdgeType.Conditional, edge.Type);
            Assert.Contains(edge, nodes[0].ConditionalEdges);
        }

        [Fact]
        public void AbnormalEdgeThroughNeighbour()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].AbnormalEdges.Add(nodes[1]);

            Assert.Equal(nodes[0], edge.Source);
            Assert.Equal(EdgeType.Abnormal, edge.Type);
            Assert.Contains(edge, nodes[0].AbnormalEdges);
        }

        [Fact]
        public void ConnectWithFallThrough()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1]);
            Assert.Equal(nodes[0], edge.Source);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(EdgeType.FallThrough, edge.Type);
            Assert.Equal(edge, nodes[0].FallThroughEdge);
        }

        [Fact]
        public void ConnectWithConditional()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1], EdgeType.Conditional);
            Assert.Equal(nodes[0], edge.Source);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(EdgeType.Conditional, edge.Type);
            Assert.Contains(edge, nodes[0].ConditionalEdges);
        }

        [Fact]
        public void ConnectWithAbnormal()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1], EdgeType.Abnormal);
            Assert.Equal(nodes[0], edge.Source);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(EdgeType.Abnormal, edge.Type);
            Assert.Contains(edge, nodes[0].AbnormalEdges);
        }

        [Fact]
        public void NeighboursExist()
        {
            var nodes = CreateDummyGraph(4);

            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], EdgeType.Conditional);
            nodes[2].ConnectWith(nodes[3]);
            
            Assert.True(nodes[0].HasNeighbour(nodes[1]));
            Assert.True(nodes[0].HasNeighbour(nodes[2]));
            Assert.False(nodes[0].HasNeighbour(nodes[3]));
            Assert.True(nodes[2].HasNeighbour(nodes[3]));
        }

        [Fact]
        public void MultiEdges()
        {
            var nodes = CreateDummyGraph(2);
            nodes[0].ConnectWith(nodes[1], EdgeType.Conditional);
            nodes[0].ConnectWith(nodes[1], EdgeType.Conditional);
            
            Assert.Equal(2, nodes[0].ConditionalEdges.Count);
            Assert.Equal(2, nodes[0].ParentGraph.GetEdges().Count());
        }

    }
}