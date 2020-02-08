using Echo.DataFlow.Values;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.DataFlow.Tests
{
    public class DataFlowNodeTest
    {
        [Fact]
        public void ConstructorShouldSetContents()
        {
            var node = new DataFlowNode<int>(1, 2);
            Assert.Equal(2, node.Contents);
        }

        [Fact]
        public void AddNodeToGraphShouldSetParentGraph()
        {
            var dfg = new DataFlowGraph<int>();
            var node = new DataFlowNode<int>(1, 2);
            dfg.Nodes.Add(node);
            Assert.Same(dfg, node.ParentGraph);
        }

        [Fact]
        public void RemoveNodeFromGraphShouldUnsetParentGraph()
        {
            var dfg = new DataFlowGraph<int>();
            var node = new DataFlowNode<int>(1, 2);
            dfg.Nodes.Add(node);
            dfg.Nodes.Remove(node);
            Assert.Null(node.ParentGraph);
        }
        
        [Fact]
        public void AddStackDependencyShouldSetDependant()
        {
            var dfg = new DataFlowGraph<int>();
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);

            var symbolicValue = new SymbolicValue<int>(n0);
            n1.StackDependencies.Add(symbolicValue);
            
            Assert.Same(n1, symbolicValue.Dependant);
        }

        [Fact]
        public void RemoveStackDependencyShouldUnsetDependant()
        {
            var dfg = new DataFlowGraph<int>();
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);

            var symbolicValue = new SymbolicValue<int>(n0);
            n1.StackDependencies.Add(symbolicValue);
            n1.StackDependencies.Remove(symbolicValue);
            Assert.Null(symbolicValue.Dependant);
        }
        
        [Fact]
        public void AddVariableDependencyShouldSetDependant()
        {
            var variable = new DummyVariable("V_1");
            
            var dfg = new DataFlowGraph<int>();
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);

            var symbolicValue = new SymbolicValue<int>(n0);
            n1.VariableDependencies.Add(variable, symbolicValue);
            
            Assert.Same(n1, symbolicValue.Dependant);
        }

        [Fact]
        public void RemoveVariableDependencyShouldUnsetDependant()
        {
            var variable = new DummyVariable("V_1");
            
            var dfg = new DataFlowGraph<int>();
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);

            var symbolicValue = new SymbolicValue<int>(n0);
            n1.VariableDependencies.Add(variable, symbolicValue);
            n1.VariableDependencies.Remove(variable);
            
            Assert.Null(symbolicValue.Dependant);
        }
    }
}