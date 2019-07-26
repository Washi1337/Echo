using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Specialized;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests
{
    public class NodeTest
    {
        private static IList<Node<int>> CreateDummyGraph(int nodeCount)
        {
            var graph = new Graph<int>();

            var nodes = new Node<int>[nodeCount];
            for (int i = 0; i < nodeCount; i++)
                nodes[i] = new Node<int>(i);

            foreach (var node in nodes)
                graph.Nodes.Add(node);

            return nodes;
        }

        [Fact]
        public void ValidFallthroughEdge()
        {
            var nodes = CreateDummyGraph(2);
            nodes[0].FallThroughEdge = new Edge<int>(nodes[0], nodes[1]);
            
            Assert.Equal(nodes[0], nodes[0].FallThroughEdge.Origin);
            Assert.Equal(nodes[1], nodes[0].FallThroughEdge.Target);
            Assert.Equal(EdgeType.FallThrough, nodes[0].FallThroughEdge.Type);
        }

        [Fact]
        public void InvalidFallthroughEdge()
        {
            var nodes = CreateDummyGraph(2);
            nodes[0].FallThroughEdge = new Edge<int>(nodes[0], nodes[1]);

            Assert.Throws<ArgumentException>(
                () => nodes[0].FallThroughEdge = new Edge<int>(nodes[1], nodes[0]));
            
            Assert.Throws<ArgumentException>(
                () => nodes[0].FallThroughEdge = new Edge<int>(nodes[0], nodes[1], EdgeType.Conditional));
        }
        
        [Fact]
        public void ConditionalEdge()
        { 
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConditionalEdges.Add(
                new Edge<int>(nodes[0], nodes[1], EdgeType.Conditional));
            Assert.Contains(edge, nodes[0].ConditionalEdges);
        }
        
        [Fact]
        public void ConditionalEdgeThroughNeighbour()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConditionalEdges.Add(nodes[1]);

            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(EdgeType.Conditional, edge.Type);
            Assert.Contains(edge, nodes[0].ConditionalEdges);
        }

        [Fact]
        public void AbnormalEdgeThroughNeighbour()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].AbnormalEdges.Add(nodes[1]);

            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(EdgeType.Abnormal, edge.Type);
            Assert.Contains(edge, nodes[0].AbnormalEdges);
        }

        [Fact]
        public void ConnectWithFallThrough()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1]);
            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(EdgeType.FallThrough, edge.Type);
            Assert.Equal(edge, nodes[0].FallThroughEdge);
        }

        [Fact]
        public void ConnectWithConditional()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1], EdgeType.Conditional);
            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(EdgeType.Conditional, edge.Type);
            Assert.Contains(edge, nodes[0].ConditionalEdges);
        }

        [Fact]
        public void ConnectWithAbnormal()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1], EdgeType.Abnormal);
            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(EdgeType.Abnormal, edge.Type);
            Assert.Contains(edge, nodes[0].AbnormalEdges);
        }

        [Fact]
        public void SuccessorExists()
        {
            var nodes = CreateDummyGraph(4);

            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], EdgeType.Conditional);
            nodes[2].ConnectWith(nodes[3]);
            
            Assert.True(nodes[0].HasSuccessor(nodes[1]));
            Assert.True(nodes[0].HasSuccessor(nodes[2]));
            Assert.False(nodes[0].HasSuccessor(nodes[3]));
            Assert.True(nodes[2].HasSuccessor(nodes[3]));
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

        [Fact]
        public void GetSuccessors()
        {
            var nodes = CreateDummyGraph(4);

            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], EdgeType.Conditional);
            nodes[1].ConnectWith(nodes[3]);
            nodes[2].ConnectWith(nodes[3]);

            Assert.Equal(new HashSet<Node<int>>
            {
                nodes[1], nodes[2]
            }, new HashSet<Node<int>>(nodes[0].GetSuccessors()));
            
            Assert.Equal(new HashSet<Node<int>>
            {
                nodes[3]
            }, new HashSet<Node<int>>(nodes[1].GetSuccessors()));
            
            Assert.Equal(new HashSet<Node<int>>
            {
                nodes[3]
            }, new HashSet<Node<int>>(nodes[2].GetSuccessors()));
            
            Assert.Empty(nodes[3].GetSuccessors());
        }

        [Fact]
        public void GetPredecessors()
        {
            var nodes = CreateDummyGraph(4);

            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], EdgeType.Conditional);
            nodes[1].ConnectWith(nodes[3]);
            nodes[2].ConnectWith(nodes[3]);

            Assert.Empty(nodes[0].GetPredecessors());

            Assert.Equal(new HashSet<Node<int>>
            {
                nodes[0]
            }, new HashSet<Node<int>>(nodes[1].GetPredecessors()));

            Assert.Equal(new HashSet<Node<int>>
            {
                nodes[0]
            }, new HashSet<Node<int>>(nodes[2].GetPredecessors()));

            Assert.Equal(new HashSet<Node<int>>
            {
                nodes[1], nodes[2]
            }, new HashSet<Node<int>>(nodes[3].GetPredecessors()));
        }

        [Fact]
        public void SplitBasicBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Op(3, 0, 0),
            };
            
            var cfg = new ControlFlowGraph<DummyInstruction>();
            var node = new BasicBlockNode<DummyInstruction>(instructions);
            cfg.Nodes.Add(node);
            cfg.Entrypoint = node;

            node.Split(2);

            Assert.Equal(2, cfg.Nodes.Count);
            Assert.Equal(new[] {instructions[0], instructions[1]}, node.Contents.Instructions);
            
            var newNode = cfg.Entrypoint.FallThroughNeighbour;
            Assert.NotNull(newNode);
            Assert.Equal(new[] {instructions[2], instructions[3]}, newNode.Contents.Instructions);
        }
        
        [Fact]
        public void SplitMultipleOutgoingEdges()
        {
            var cfg = TestGraphs.CreateIf();
            var nodes = cfg.Nodes
                .OrderBy(n => n.Contents.Offset)
                .ToArray();

            nodes[0].Split(1);
            var newNode = nodes[0].FallThroughNeighbour;
            
            Assert.NotEqual(nodes[2], newNode);
            Assert.Equal(nodes[1], newNode.FallThroughNeighbour);
            Assert.Contains(nodes[2], newNode.ConditionalEdges.Select(e => e.Target));
        }

        [Fact]
        public void SplitMultipleIncomingEdges()
        {
            var cfg = TestGraphs.CreateIf();
            var nodes = cfg.Nodes
                .OrderBy(n => n.Contents.Offset)
                .ToArray();

            nodes[2].Split(1);
            var newNode = nodes[2].FallThroughNeighbour;

            Assert.NotNull(newNode);
            Assert.Equal(2, nodes[2].GetIncomingEdges().Count());
            Assert.Single(newNode.GetIncomingEdges());
            Assert.True(newNode.HasPredecessor(nodes[2]));
        }
    }
}