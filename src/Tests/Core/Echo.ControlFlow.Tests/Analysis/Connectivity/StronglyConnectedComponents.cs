using System.Collections.Generic;
using Echo.ControlFlow.Analysis.Connectivity;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Analysis.Connectivity
{
    public class StronglyConnectedComponents
    {
        [Fact]
        public void Simple()
        {
            var cfg = new Graph<DummyInstruction>();

            var nodes = new Node<DummyInstruction>[5];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Node<DummyInstruction>();
                cfg.Nodes.Add(nodes[i]);
            }

            nodes[0].ConnectWith(nodes[2]);
            nodes[2].ConnectWith(nodes[1]);
            nodes[1].ConnectWith(nodes[0]);
            nodes[0].ConnectWith(nodes[3], EdgeType.Conditional);
            nodes[3].ConnectWith(nodes[4]);

            cfg.Entrypoint = nodes[0];
            
            var components = cfg.FindStronglyConnectedComponents();

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
    }
}