using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Regions;
using Xunit;
using Echo.Platforms.DummyPlatform;

namespace Echo.ControlFlow.Tests
{
    public class NodeTest
    {
        private static IList<ControlFlowNode<int>> CreateDummyGraph(int nodeCount)
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = new ControlFlowNode<int>[nodeCount];
            for (int i = 0; i < nodeCount; i++)
                nodes[i] = new ControlFlowNode<int>(i, i);

            foreach (var node in nodes)
                graph.Nodes.Add(node);

            return nodes;
        }

        [Fact]
        public void ValidFallthroughEdge()
        {
            var nodes = CreateDummyGraph(2);
            nodes[0].UnconditionalEdge = new ControlFlowEdge<int>(nodes[0], nodes[1]);
            
            Assert.Equal(nodes[0], nodes[0].UnconditionalEdge.Origin);
            Assert.Equal(nodes[1], nodes[0].UnconditionalEdge.Target);
            Assert.Equal(ControlFlowEdgeType.FallThrough, nodes[0].UnconditionalEdge.Type);
            Assert.Equal(1, nodes[0].OutDegree);
            Assert.Equal(1, nodes[1].InDegree);
        }

        [Fact]
        public void InvalidFallthroughEdge()
        {
            var nodes = CreateDummyGraph(2);
            
            Assert.Throws<ArgumentException>(
                () => nodes[0].UnconditionalEdge = new ControlFlowEdge<int>(nodes[1], nodes[0]));
            
            Assert.Throws<ArgumentException>(
                () => nodes[0].UnconditionalEdge = new ControlFlowEdge<int>(nodes[0], nodes[1], ControlFlowEdgeType.Conditional));
            
            Assert.Equal(0, nodes[0].OutDegree);
            Assert.Equal(0, nodes[1].InDegree);
        }
        
        [Fact]
        public void ConditionalEdge()
        { 
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConditionalEdges.Add(
                new ControlFlowEdge<int>(nodes[0], nodes[1], ControlFlowEdgeType.Conditional));
            
            Assert.Contains(edge, nodes[0].ConditionalEdges);
            Assert.Equal(1, nodes[0].OutDegree);
            Assert.Equal(1, nodes[1].InDegree);
        }
        
        [Fact]
        public void ConditionalEdgeThroughNeighbour()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConditionalEdges.Add(nodes[1]);

            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(ControlFlowEdgeType.Conditional, edge.Type);
            Assert.Contains(edge, nodes[0].ConditionalEdges);
            Assert.Equal(1, nodes[0].OutDegree);
            Assert.Equal(1, nodes[1].InDegree);
        }

        [Fact]
        public void AbnormalEdgeThroughNeighbour()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].AbnormalEdges.Add(nodes[1]);

            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(ControlFlowEdgeType.Abnormal, edge.Type);
            Assert.Contains(edge, nodes[0].AbnormalEdges);
            Assert.Equal(1, nodes[0].OutDegree);
            Assert.Equal(1, nodes[1].InDegree);
        }

        [Fact]
        public void ConnectWithFallThrough()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1]);
            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(ControlFlowEdgeType.FallThrough, edge.Type);
            Assert.Equal(edge, nodes[0].UnconditionalEdge);
        }

        [Fact]
        public void ConnectWithConditional()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1], ControlFlowEdgeType.Conditional);
            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(ControlFlowEdgeType.Conditional, edge.Type);
            Assert.Contains(edge, nodes[0].ConditionalEdges);
        }

        [Fact]
        public void ConnectWithAbnormal()
        {
            var nodes = CreateDummyGraph(2);

            var edge = nodes[0].ConnectWith(nodes[1], ControlFlowEdgeType.Abnormal);
            Assert.Equal(nodes[0], edge.Origin);
            Assert.Equal(nodes[1], edge.Target);
            Assert.Equal(ControlFlowEdgeType.Abnormal, edge.Type);
            Assert.Contains(edge, nodes[0].AbnormalEdges);
        }

        [Fact]
        public void SuccessorExists()
        {
            var nodes = CreateDummyGraph(4);

            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);
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
            nodes[0].ConnectWith(nodes[1], ControlFlowEdgeType.Conditional);
            nodes[0].ConnectWith(nodes[1], ControlFlowEdgeType.Conditional);
            
            Assert.Equal(2, nodes[0].ConditionalEdges.Count);
            Assert.Equal(2, nodes[0].ParentGraph.GetEdges().Count());
            Assert.Equal(2, nodes[0].OutDegree);
            Assert.Equal(2, nodes[1].InDegree);
        }

        [Fact]
        public void GetSuccessors()
        {
            var nodes = CreateDummyGraph(4);

            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);
            nodes[1].ConnectWith(nodes[3]);
            nodes[2].ConnectWith(nodes[3]);

            Assert.Equal(new HashSet<ControlFlowNode<int>>
            {
                nodes[1], nodes[2]
            }, new HashSet<ControlFlowNode<int>>(nodes[0].GetSuccessors()));
            
            Assert.Equal(new HashSet<ControlFlowNode<int>>
            {
                nodes[3]
            }, new HashSet<ControlFlowNode<int>>(nodes[1].GetSuccessors()));
            
            Assert.Equal(new HashSet<ControlFlowNode<int>>
            {
                nodes[3]
            }, new HashSet<ControlFlowNode<int>>(nodes[2].GetSuccessors()));
            
            Assert.Empty(nodes[3].GetSuccessors());
        }

        [Fact]
        public void GetPredecessors()
        {
            var nodes = CreateDummyGraph(4);

            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);
            nodes[1].ConnectWith(nodes[3]);
            nodes[2].ConnectWith(nodes[3]);

            Assert.Empty(nodes[0].GetPredecessors());

            Assert.Equal(new HashSet<ControlFlowNode<int>>
            {
                nodes[0]
            }, new HashSet<ControlFlowNode<int>>(nodes[1].GetPredecessors()));

            Assert.Equal(new HashSet<ControlFlowNode<int>>
            {
                nodes[0]
            }, new HashSet<ControlFlowNode<int>>(nodes[2].GetPredecessors()));

            Assert.Equal(new HashSet<ControlFlowNode<int>>
            {
                nodes[1], nodes[2]
            }, new HashSet<ControlFlowNode<int>>(nodes[3].GetPredecessors()));
        }

        [Fact]
        public void SplitNodeShouldConnectUsingFallThrough()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0, 1, 2, 3, 4);
            graph.Nodes.Add(node);

            var (first, second) = node.SplitAtIndex(2);

            Assert.Equal(new[] { 0, 1 }, first.Contents.Instructions);
            Assert.Equal(new[] { 2, 3, 4 }, second.Contents.Instructions);
            Assert.Same(first.UnconditionalNeighbour, second);
        }

        [Fact]
        public void SplitEmptyNodeShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>([]);
            graph.Nodes.Add(node);
            Assert.Throws<InvalidOperationException>(() => node.SplitAtIndex(1));
        }

        [Fact]
        public void SplitNodeWithSingleInstructionShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0);
            graph.Nodes.Add(node);
            Assert.Throws<InvalidOperationException>(() => node.SplitAtIndex(0));
        }

        [Fact]
        public void SplitNodeAtHeaderShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0, 1, 2);
            graph.Nodes.Add(node);
            Assert.Throws<ArgumentOutOfRangeException>(() => node.SplitAtIndex(0));
        }

        [Fact]
        public void SplitNodeAtFooterShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0, 1, 2);
            graph.Nodes.Add(node);
            Assert.Throws<ArgumentOutOfRangeException>(() => node.SplitAtIndex(3));
        }

        [Fact]
        public void SplitNodeShouldTransferFallThroughEdge()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = new ControlFlowNode<int>[]
            {
                new(0),
                new(1, 2),
                new(3),
            };
            
            graph.Nodes.AddRange(nodes);

            nodes[0].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[2]);
            
            var (first, second) = nodes[1].SplitAtIndex(1);
            
            Assert.Same(nodes[2], second.UnconditionalNeighbour); 
        }

        [Fact]
        public void SplitNodeShouldTransferConditionalEdges()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = new ControlFlowNode<int>[]
            {
                new(0),
                new(1, 2),
                new(3),
                new(4),
            };
            
            graph.Nodes.AddRange(nodes);

            nodes[0].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);
            nodes[1].ConnectWith(nodes[3]);
            
            var (first, second) = nodes[1].SplitAtIndex(1);
            
            Assert.Same(nodes[3], second.UnconditionalNeighbour);
            Assert.Contains(nodes[2], second.ConditionalEdges.Select(e => e.Target));
        }

        [Fact]
        public void SplitSelfLoopNode()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var node = new ControlFlowNode<int>(0, 1);
            graph.Nodes.Add(node);
            node.UnconditionalNeighbour = node;

            var (first, second) = node.SplitAtIndex(1);
            
            Assert.Same(second, first.UnconditionalNeighbour);
            Assert.Same(first, second.UnconditionalNeighbour);
        }

        [Fact]
        public void MergeWithSuccessorShouldRemoveSuccessor()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var nodes = new ControlFlowNode<int>[]
            {
                new(0),
                new(1),
            };
            graph.Nodes.AddRange(nodes);
            nodes[0].ConnectWith(nodes[1]);
            
            nodes[0].MergeWithSuccessor();

            Assert.Equal([nodes[0]], graph.Nodes.ToArray());
        }

        [Fact]
        public void MergeWithSuccessorShouldCombineInstructions()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var nodes = new ControlFlowNode<int>[]
            {
                new(0),
                new(1),
            };
            graph.Nodes.AddRange(nodes);
            nodes[0].ConnectWith(nodes[1]);
            
            nodes[0].MergeWithSuccessor();

            Assert.Equal([0, 1], nodes[0].Contents.Instructions);
        }

        [Fact]
        public void MergeWithSuccessorShouldInheritFallThroughEdge()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var nodes = new ControlFlowNode<int>[]
            {
                new(0),
                new(1),
                new(2),
            };
            graph.Nodes.AddRange(nodes);
            nodes[0].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[2]);
            
            nodes[0].MergeWithSuccessor();
            
            Assert.Same(nodes[2], nodes[0].UnconditionalNeighbour);
        }

        [Fact]
        public void MergeWithSuccessorShouldInheritConditionalEdge()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var nodes = new ControlFlowNode<int>[]
            {
                new(0),
                new(1),
                new(2),
            };
            graph.Nodes.AddRange(nodes);
            nodes[0].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);
            
            nodes[0].MergeWithSuccessor();

            Assert.Equal([nodes[2]], nodes[0].ConditionalEdges.Select(e => e.Target));
        }

        [Fact]
        public void MergeWithSuccessorWithNoFallThroughNeighbourShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0);
            graph.Nodes.Add(node);

            Assert.Throws<InvalidOperationException>(() => node.MergeWithSuccessor());
        }

        [Fact]
        public void MergeWithSuccessorWithConditionalEdgesShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var nodes = new ControlFlowNode<int>[]
            {
                new(0),
                new(1),
                new(2),
            };
            graph.Nodes.AddRange(nodes);
            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);

            Assert.Throws<InvalidOperationException>(() => nodes[0].MergeWithSuccessor());
        }

        [Theory]
        [InlineData(ControlFlowEdgeType.FallThrough, false)]
        [InlineData(ControlFlowEdgeType.FallThrough, true)]
        [InlineData(ControlFlowEdgeType.Conditional, false)]
        [InlineData(ControlFlowEdgeType.Conditional, true)]
        [InlineData(ControlFlowEdgeType.Abnormal, false)]
        [InlineData(ControlFlowEdgeType.Abnormal, true)]
        public void DisconnectNodeShouldRemoveEdge(ControlFlowEdgeType edgeType, bool removeSourceNode)
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var n1 = new ControlFlowNode<int>(0, 0);
            var n2 = new ControlFlowNode<int>(1, 1);

            graph.Nodes.AddRange(new[]
            {
                n1,
                n2
            });

            n1.ConnectWith(n2, edgeType);
            
            if (removeSourceNode)
                n1.Disconnect();
            else
                n2.Disconnect();

            Assert.Empty(n1.GetOutgoingEdges());
            Assert.Empty(n2.GetIncomingEdges());
        }

        [Fact]
        public void SplittingNodeShouldPreserveParentRegion()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var n1 = new ControlFlowNode<int>(0, 0, 2, 3, 4);
            var n2 = new ControlFlowNode<int>(5, 5, 6, 7, 8);
            
            var region = new ExceptionHandlerRegion<int>();
            graph.Regions.Add(region);
            
            var handler = new HandlerRegion<int>();
            region.Handlers.Add(handler);

            graph.Nodes.AddRange(new[] { n1, n2 });
            region.ProtectedRegion.Nodes.Add(n1);
            handler.Contents.Nodes.Add(n2);

            n1.ConnectWith(n2, ControlFlowEdgeType.Abnormal);
            graph.EntryPoint = n1;

            var (first, second) = n1.SplitAtIndex(2);
            Assert.Equal(first.ParentRegion, second.ParentRegion);
            Assert.Equal(first.ParentRegion, region.ProtectedRegion);
            Assert.Equal(2, region.ProtectedRegion.Nodes.Count);
        }
    }
}