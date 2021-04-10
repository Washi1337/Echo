using System.Collections.Generic;
using Echo.ControlFlow.Analysis.Domination;
using Echo.Core.Graphing;
using Echo.Platforms.DummyPlatform;
using Xunit;

namespace Echo.ControlFlow.Tests.Analysis.Domination
{
    public class DominanceFrontierTest
    {
        [Fact]
        public void Simple()
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);
            
            var n = new ControlFlowNode<int>(0, 0);
            cfg.Nodes.Add(n);
            cfg.Entrypoint = n;

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.Empty(tree.GetDominanceFrontier(n));
        }

        [Fact]
        public void Path()
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = new ControlFlowNode<int>[3];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new ControlFlowNode<int>(i, i);
                cfg.Nodes.Add(nodes[i]);
                if (i > 0)
                    nodes[i - 1].ConnectWith(nodes[i]);
            }

            cfg.Entrypoint = nodes[0];

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.All(nodes, n => Assert.Empty(tree.GetDominanceFrontier(n)));
        }

        [Fact]
        public void If()
        {
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = new ControlFlowNode<int>[4];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new ControlFlowNode<int>(i, i);
                cfg.Nodes.Add(nodes[i]);
            }

            nodes[0].ConnectWith(nodes[1]);
            nodes[0].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);
            nodes[1].ConnectWith(nodes[3]);
            nodes[2].ConnectWith(nodes[3]);

            cfg.Entrypoint = nodes[0];

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.Equal(new[] {nodes[3]}, tree.GetDominanceFrontier(nodes[1]));
            Assert.Equal(new[] {nodes[3]}, tree.GetDominanceFrontier(nodes[2]));
        }

        [Fact]
        public void Loop()
        {        
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = new ControlFlowNode<int>[4];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new ControlFlowNode<int>(i, i);
                cfg.Nodes.Add(nodes[i]);
            }

            nodes[0].ConnectWith(nodes[2]);
            nodes[1].ConnectWith(nodes[2]);
            nodes[2].ConnectWith(nodes[1], ControlFlowEdgeType.Conditional);
            nodes[2].ConnectWith(nodes[3]);

            cfg.Entrypoint = nodes[0];

            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.Equal(new HashSet<INode> {nodes[2]}, tree.GetDominanceFrontier(nodes[1]));
            Assert.Equal(new HashSet<INode> {nodes[2]}, tree.GetDominanceFrontier(nodes[2]));
        }

        [Fact]
        public void Complex()
        {
            // Example graph from:
            // http://www.sable.mcgill.ca/~hendren/621/ControlFlowAnalysis_Handouts.pdf
            // (slide 57)
            
            var cfg = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var nodes = new ControlFlowNode<int>[11];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new ControlFlowNode<int>(i, i);
                cfg.Nodes.Add(nodes[i]);
            }

            nodes[0].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[2], ControlFlowEdgeType.Conditional);
            nodes[1].ConnectWith(nodes[3]);
            nodes[2].ConnectWith(nodes[3]);
            nodes[3].ConnectWith(nodes[4]);
            nodes[4].ConnectWith(nodes[3], ControlFlowEdgeType.Conditional);
            nodes[4].ConnectWith(nodes[5]);
            nodes[4].ConnectWith(nodes[6], ControlFlowEdgeType.Conditional);
            nodes[5].ConnectWith(nodes[7]);
            nodes[6].ConnectWith(nodes[7]);
            nodes[7].ConnectWith(nodes[8]);
            nodes[7].ConnectWith(nodes[4], ControlFlowEdgeType.Conditional);
            nodes[8].ConnectWith(nodes[9]);
            nodes[8].ConnectWith(nodes[10], ControlFlowEdgeType.Conditional);
            nodes[8].ConnectWith(nodes[3], ControlFlowEdgeType.Conditional);
            nodes[9].ConnectWith(nodes[1]);
            nodes[10].ConnectWith(nodes[7]);

            cfg.Entrypoint = nodes[0];
            
            var tree = DominatorTree<int>.FromGraph(cfg);
            Assert.Empty(tree.GetDominanceFrontier(nodes[0]));
            Assert.Equal(new HashSet<INode> {nodes[1]}, tree.GetDominanceFrontier(nodes[1]));
            Assert.Equal(new HashSet<INode> {nodes[3]}, tree.GetDominanceFrontier(nodes[2]));
            Assert.Equal(new HashSet<INode> {nodes[1], nodes[3]}, tree.GetDominanceFrontier(nodes[3]));
            Assert.Equal(new HashSet<INode> {nodes[1], nodes[3], nodes[4]}, tree.GetDominanceFrontier(nodes[4]));
            Assert.Equal(new HashSet<INode> {nodes[7]}, tree.GetDominanceFrontier(nodes[5]));
            Assert.Equal(new HashSet<INode> {nodes[7]}, tree.GetDominanceFrontier(nodes[6]));
            Assert.Equal(new HashSet<INode> {nodes[1], nodes[3], nodes[4], nodes[7]}, tree.GetDominanceFrontier(nodes[7]));
            Assert.Equal(new HashSet<INode> {nodes[1], nodes[3], nodes[7]}, tree.GetDominanceFrontier(nodes[8]));
            Assert.Equal(new HashSet<INode> {nodes[1]}, tree.GetDominanceFrontier(nodes[9]));
            Assert.Equal(new HashSet<INode> {nodes[7]}, tree.GetDominanceFrontier(nodes[10]));
        }
    }
}