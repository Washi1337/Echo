using System;
using System.Collections.Generic;
using Echo.Platforms.DummyPlatform;
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
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var node = new DataFlowNode<int>(1, 2);
            dfg.Nodes.Add(node);
            Assert.Same(dfg, node.ParentGraph);
        }

        [Fact]
        public void RemoveNodeFromGraphShouldUnsetParentGraph()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var node = new DataFlowNode<int>(1, 2);
            dfg.Nodes.Add(node);
            dfg.Nodes.Remove(node);
            Assert.Null(node.ParentGraph);
        }
        
        [Fact]
        public void AddStackDependencyShouldSetDependant()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);

            var dependency = new DataDependency<int>(n0);
            n1.StackDependencies.Add(dependency);
            
            Assert.Same(n1, dependency.Dependent);
        }

        [Fact]
        public void RemoveStackDependencyShouldUnsetDependant()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);

            var symbolicValue = new DataDependency<int>(n0);
            n1.StackDependencies.Add(symbolicValue);
            n1.StackDependencies.Remove(symbolicValue);
            Assert.Null(symbolicValue.Dependent);
        }

        [Fact]
        public void AddStackDependencyShouldAddToDependants()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);

            var dependency1 = new DataDependency<int>(n0);
            n1.StackDependencies.Add(dependency1);
            var dependency2 = new DataDependency<int>(n0);
            n2.StackDependencies.Add(dependency2);

            Assert.Equal(new HashSet<DataFlowNode<int>>
            {
                n1, n2
            }, new HashSet<DataFlowNode<int>>(n0.GetDependants()));
        }

        [Fact]
        public void RemoveStackDependencyShouldRemoveFromDependants()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n0 = dfg.Nodes.Add(0, 0);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);

            var dependency1 = new DataDependency<int>(n0);
            n1.StackDependencies.Add(dependency1);
            var dependency2 = new DataDependency<int>(n0);
            n2.StackDependencies.Add(dependency2);

            n1.StackDependencies.Remove(dependency1);

            Assert.Equal(new HashSet<DataFlowNode<int>>
            {
                n2
            }, new HashSet<DataFlowNode<int>>(n0.GetDependants()));
        }

        [Fact]
        public void AddDependencyToAnotherGraphShouldThrow()
        {
            var dfg1 = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg1.Nodes.Add(1, 0);
            
            var dfg2 = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n2 = dfg2.Nodes.Add(2, 0);

            Assert.Throws<ArgumentException>(() =>
                n1.StackDependencies.Add(new DataDependency<int>(n2)));
        }

        [Fact]
        public void AddDataSourceToAnotherGraphShouldThrow()
        {
            var dfg1 = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg1.Nodes.Add(1, 0);
            
            var dfg2 = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n2 = dfg2.Nodes.Add(2, 0);

            n1.StackDependencies.Add(new DataDependency<int>());
            Assert.Throws<ArgumentException>(() => n1.StackDependencies[0].Add(new DataSource<int>(n2)));
        }

        [Fact]
        public void RemoveNodeShouldRemoveStackDeps()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 0);
            var n2 = dfg.Nodes.Add(2, 0);
            
            n1.StackDependencies.Add(new DataDependency<int>(n2));
            
            Assert.Single(n1.StackDependencies[0]);
            Assert.Single(n2.GetDependants());
            
            dfg.Nodes.Remove(n2);
            
            Assert.Empty(n1.StackDependencies[0]);
            Assert.Empty(n2.GetDependants());
        }

        [Fact]
        public void RemoveNodeShouldRemoveStackDependants()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 0);
            var n2 = dfg.Nodes.Add(2, 0);
            
            n1.StackDependencies.Add(new DataDependency<int>(n2));
            
            Assert.Single(n1.StackDependencies[0]);
            Assert.Single(n2.GetDependants());
            
            dfg.Nodes.Remove(n1);
            
            Assert.Empty(n1.StackDependencies[0]);
            Assert.Empty(n2.GetDependants());
        }

        [Fact]
        public void RemoveNodeShouldRemoveVarDeps()
        {
            var variable = new DummyVariable("V_1");
            
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 0);
            var n2 = dfg.Nodes.Add(2, 0);

            n1.VariableDependencies[variable] = new DataDependency<int>(n2);
            
            Assert.Single(n1.VariableDependencies[variable]);
            Assert.Single(n2.GetDependants());
            
            dfg.Nodes.Remove(n2);
            
            Assert.Empty(n1.VariableDependencies[variable]);
            Assert.Empty(n2.GetDependants());
        }
        
        [Theory]
        [InlineData(DataDependencyType.Stack, false)]
        [InlineData(DataDependencyType.Stack, true)]
        [InlineData(DataDependencyType.Variable, false)]
        [InlineData(DataDependencyType.Variable, true)]
        public void DisconnectNodeShouldRemoveEdge(DataDependencyType edgeType, bool removeSourceNode)
        {
            var variable = new DummyVariable("var");
            
            var graph = new DataFlowGraph<int>(IntArchitecture.Instance);

            var n1 = new DataFlowNode<int>(0, 0);
            n1.StackDependencies.Add(new DataDependency<int>());
            n1.VariableDependencies.Add(variable, new DataDependency<int>());
            
            var n2 = new DataFlowNode<int>(1, 1);

            graph.Nodes.AddRange(new[]
            {
                n1,
                n2
            });

            switch (edgeType)
            {
                case DataDependencyType.Stack:
                    n1.StackDependencies[0].Add(new DataSource<int>(n2));
                    break;
                case DataDependencyType.Variable:
                    n1.VariableDependencies[variable].Add(new DataSource<int>(n2));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edgeType), edgeType, null);
            }
            
            if (removeSourceNode)
                n1.Disconnect();
            else
                n2.Disconnect();

            Assert.Empty(n1.StackDependencies[0]);
            Assert.Empty(n1.VariableDependencies[variable]);
            Assert.Empty(n2.GetDependants());
        }

        [Fact]
        public void AddStackDependencyWithSingeSourceShouldSetDegree()
        {
            var graph = new DataFlowGraph<int>(IntArchitecture.Instance);

            var n1 = graph.Nodes.Add(1, 1);
            var n2 = graph.Nodes.Add(2, 2);
            
            Assert.Equal(0, n1.OutDegree);
            Assert.Equal(0, n2.InDegree);
            n1.StackDependencies.Add(new DataDependency<int>(n2));
            Assert.Equal(1, n1.OutDegree);
            Assert.Equal(1, n2.InDegree);
        }

        [Fact]
        public void AddStackDependencyWithMultipleSourcesShouldSetDegree()
        {
            var graph = new DataFlowGraph<int>(IntArchitecture.Instance);

            var n1 = graph.Nodes.Add(1, 1);
            var n2 = graph.Nodes.Add(2, 2);
            var n3 = graph.Nodes.Add(3, 3);
            
            Assert.Equal(0, n1.OutDegree);
            n1.StackDependencies.Add(new DataDependency<int>(new[] {n2, n3}));
            Assert.Equal(2, n1.OutDegree);
        }

        [Fact]
        public void AddVariableDependencyWithSingeSourceShouldSetDegree()
        {
            var variable = new DummyVariable("var1");
            var graph = new DataFlowGraph<int>(IntArchitecture.Instance);

            var n1 = graph.Nodes.Add(1, 1);
            var n2 = graph.Nodes.Add(2, 2);
            
            Assert.Equal(0, n1.OutDegree);
            n1.VariableDependencies.Add(variable, new DataDependency<int>(n2));
            Assert.Equal(1, n1.OutDegree);
            Assert.Equal(1, n2.InDegree);
        }

        [Fact]
        public void AddVariableDependencyWithMultipleSourcesShouldSetDegree()
        {
            var variable = new DummyVariable("var1");
            var graph = new DataFlowGraph<int>(IntArchitecture.Instance);

            var n1 = graph.Nodes.Add(1, 1);
            var n2 = graph.Nodes.Add(2, 2);
            var n3 = graph.Nodes.Add(3, 3);
            
            Assert.Equal(0, n1.OutDegree);
            n1.VariableDependencies.Add(variable, new DataDependency<int>(new[]{n2, n3}));
            Assert.Equal(2, n1.OutDegree);
        }
    }
}