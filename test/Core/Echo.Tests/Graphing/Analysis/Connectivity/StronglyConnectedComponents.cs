using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.Graphing;
using Echo.Graphing.Analysis.Connectivity;
using Xunit;

namespace Echo.Tests.Graphing.Analysis.Connectivity
{
    public class StronglyConnectedComponents
    {
        [Fact]
        public void Simple()
        {
            var graph = new IntGraph();

            var nodes = new IntNode[5];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = graph.AddNode(i);

            nodes[0].ConnectWith(nodes[2]);
            nodes[2].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[0]);
            nodes[0].ConnectWith(nodes[3]);
            nodes[3].ConnectWith(nodes[4]);
            
            var components = graph.FindStronglyConnectedComponents();

            Assert.Equal(3, components.Count);
            Assert.Contains(new HashSet<INode>
            {
                nodes[0],
                nodes[1],
                nodes[2]
            }, components);

            Assert.Contains(new HashSet<INode>
            {
                nodes[3]
            }, components);

            Assert.Contains(new HashSet<INode>
            {
                nodes[4]
            }, components);
        }

        [Fact]
        public void IsolatedNodesShouldResultInSeparateComponents()
        {
            var graph = new IntGraph();

            var nodes = new IntNode[5];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = graph.AddNode(i);
            
            var components = graph.FindStronglyConnectedComponents();
            Assert.Equal(nodes.Length, components.Count);
        }
        
        [Fact]
        public void DisconnectedComponents()
        {
            var graph = new IntGraph();

            var nodes = new IntNode[5];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = graph.AddNode(i);

            nodes[0].ConnectWith(nodes[2]);
            nodes[2].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[0]);
            
            nodes[3].ConnectWith(nodes[4]);
            nodes[4].ConnectWith(nodes[3]);
            
            var components = graph.FindStronglyConnectedComponents();

            Assert.Equal(2, components.Count);
            Assert.Contains(new HashSet<INode>
            {
                nodes[0],
                nodes[1],
                nodes[2]
            }, components);

            Assert.Contains(new HashSet<INode>
            {
                nodes[3],
                nodes[4]
            }, components);

        }

    }
}