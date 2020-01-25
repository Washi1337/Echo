using System;
using Echo.Core.Graphing;
using Echo.DataFlow.Values;
using Xunit;

namespace Echo.DataFlow.Tests
{
    public class DataFlowGraphTest
    {
        [Fact]
        public void EmptyGraph()
        {
            var dfg = new DataFlowGraph<int>();
            Assert.Empty(dfg.Nodes);
            Assert.Empty(dfg.Nodes);
        }

        [Fact]
        public void AddSingleNode()
        {
            var dfg = new DataFlowGraph<int>();
            dfg.Nodes.Add(0, 0);
            Assert.Single(dfg.Nodes);
        }

        [Fact]
        public void AddDuplicateNodeShouldNotAppearInNodes()
        {
            var dfg = new DataFlowGraph<int>();
            var node = dfg.Nodes.Add(0, 0);
            dfg.Nodes.Add(node);
            Assert.Single(dfg.Nodes);
        }

        [Fact]
        public void AddNodeWithSameIdentifierShouldThrow()
        {
            var dfg = new DataFlowGraph<int>();
            dfg.Nodes.Add(0, 0);
            Assert.Throws<ArgumentException>(() => dfg.Nodes.Add(0, 0));
        }

        [Fact]
        public void AddFromAnotherGraphShouldThrow()
        {
            var dfg1 = new DataFlowGraph<int>();
            var n1 = dfg1.Nodes.Add(0, 0);
            
            var dfg2 = new DataFlowGraph<int>();
            var n2 = dfg2.Nodes.Add(1, 1);

            Assert.Throws<ArgumentException>(() => dfg1.Nodes.Add(n2));
        }
        
        [Fact]
        public void EmptyDependencyShouldNotResultInAnEdge()
        {
            var dfg = new DataFlowGraph<int>();
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);
            
            n1.StackDependencies.Add(new SymbolicValue<int>());
            Assert.Empty(dfg.GetEdges());
        }
        
        [Fact]
        public void DependencyShouldResultInAnEdge()
        {
            var dfg = new DataFlowGraph<int>();
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);
            
            n1.StackDependencies.Add(new SymbolicValue<int>(n0));
            Assert.Contains(dfg.GetEdges(), e => e.Origin == n1 && e.Target == n0);
        }
    }
}