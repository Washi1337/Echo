using System;
using Echo.Platforms.DummyPlatform;
using Xunit;

namespace Echo.DataFlow.Tests
{
    public class DataFlowGraphTest
    {
        [Fact]
        public void EmptyGraph()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            Assert.Empty(dfg.Nodes);
            Assert.Empty(dfg.Nodes);
        }

        [Fact]
        public void AddSingleNode()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            dfg.Nodes.Add(0, 0);
            Assert.Single(dfg.Nodes);
        }

        [Fact]
        public void AddDuplicateNodeShouldNotAppearInNodes()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var node = dfg.Nodes.Add(0, 0);
            dfg.Nodes.Add(node);
            Assert.Single(dfg.Nodes);
        }

        [Fact]
        public void AddNodeWithSameIdentifierShouldThrow()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            dfg.Nodes.Add(0, 0);
            Assert.Throws<ArgumentException>(() => dfg.Nodes.Add(0, 0));
        }

        [Fact]
        public void AddFromAnotherGraphShouldThrow()
        {
            var dfg1 = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg1.Nodes.Add(0, 0);
            
            var dfg2 = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n2 = dfg2.Nodes.Add(1, 1);

            Assert.Throws<ArgumentException>(() => dfg1.Nodes.Add(n2));
        }
        
        [Fact]
        public void EmptyDependencyShouldNotResultInAnEdge()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);

            n1.StackDependencies.Add(new DataDependency<int>());
            Assert.Empty(dfg.GetEdges());
        }
        
        [Fact]
        public void DependencyShouldResultInAnEdge()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);
            
            n1.StackDependencies.Add(new DataDependency<int>(n0));
            Assert.Contains(dfg.GetEdges(), e => e.Origin == n1 && e.Target == n0);
        }
    }
}