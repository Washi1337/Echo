﻿using System.Collections.Generic;
using Echo.DataFlow.Analysis;
using Echo.Platforms.DummyPlatform;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.DataFlow.Tests.Analysis
{
    public class DependencyCollectionTest
    {
        [Fact]
        public void NoDependencies()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);

            Assert.Equal(new[]
            {
                n1
            }, n1.GetOrderedDependencies());
        }

        [Fact]
        public void SingleStackDependency()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);

            n2.StackDependencies.Add(new DataDependency<int>(n1));

            Assert.Equal(new[]
            {
                n1, n2
            }, n2.GetOrderedDependencies());
        }

        [Fact]
        public void MultipleStackDependenciesShouldResultInOrder()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3 , 3);

            n3.StackDependencies.Add(new DataDependency<int>(n1));
            n3.StackDependencies.Add(new DataDependency<int>(n2));

            Assert.Equal(new[]
            {
                n1, n2, n3
            }, n3.GetOrderedDependencies());
        }

        [Fact]
        public void PathStackDependencyGraphShouldResultInOrder()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3, 3);

            n3.StackDependencies.Add(new DataDependency<int>(n2));
            n2.StackDependencies.Add(new DataDependency<int>(n1));

            Assert.Equal(new[]
            {
                n1, n2, n3
            }, n3.GetOrderedDependencies());
        }

        [Fact]
        public void TreeStackDependencyGraphShouldResultInOrder()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3, 3);
            var n4 = dfg.Nodes.Add(4, 4);
            var n5 = dfg.Nodes.Add(5, 5);

            n5.StackDependencies.Add(new DataDependency<int>(n3));
            n5.StackDependencies.Add(new DataDependency<int>(n4));
            n3.StackDependencies.Add(new DataDependency<int>(n1));
            n3.StackDependencies.Add(new DataDependency<int>(n2));
            
            Assert.Equal(new[]
            {
                n1, n2, n3, n4, n5
            }, n5.GetOrderedDependencies());
        }

        [Fact]
        public void MirroredTreeStackDependencyGraphShouldResultInOrder()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3, 3);
            var n4 = dfg.Nodes.Add(4, 4);
            var n5 = dfg.Nodes.Add(5, 5);

            n5.StackDependencies.Add(new DataDependency<int>(n3));
            n5.StackDependencies.Add(new DataDependency<int>(n4));
            n4.StackDependencies.Add(new DataDependency<int>(n1));
            n4.StackDependencies.Add(new DataDependency<int>(n2));
            
            Assert.Equal(new[]
            {
                n3, n1, n2, n4, n5
            }, n5.GetOrderedDependencies());
        }

        [Fact]
        public void ConvergingEvenDependencyPathsShouldFinishSubPathsBeforeGoingDeeper()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3, 3);
            var n4 = dfg.Nodes.Add(4, 4);

            n4.StackDependencies.Add(new DataDependency<int>(n2));
            n4.StackDependencies.Add(new DataDependency<int>(n3));
            n3.StackDependencies.Add(new DataDependency<int>(n1));
            n2.StackDependencies.Add(new DataDependency<int>(n1));
            
            Assert.Equal(new[]
            {
                n1, n2, n3, n4
            }, n4.GetOrderedDependencies());
        }

        [Fact]
        public void ConvergingShortLongDependencyPathsShouldFinishSubPathsBeforeGoingDeeper()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3, 3);
            var n4 = dfg.Nodes.Add(4, 4);
            var n5 = dfg.Nodes.Add(5, 5);
            var n6 = dfg.Nodes.Add(6, 6);

            n6.StackDependencies.Add(new DataDependency<int>(n4));
            n6.StackDependencies.Add(new DataDependency<int>(n5));
            n5.StackDependencies.Add(new DataDependency<int>(n3));
            n3.StackDependencies.Add(new DataDependency<int>(n2));
            n2.StackDependencies.Add(new DataDependency<int>(n1));
            n4.StackDependencies.Add(new DataDependency<int>(n1));
            
            Assert.Equal(new[]
            {
                n1, n4, n2, n3, n5, n6
            }, n6.GetOrderedDependencies());
        }

        [Fact]
        public void SelfLoopShouldThrowCyclicDependencyException()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            n1.StackDependencies.Add(new DataDependency<int>(n1));

            Assert.Throws<CyclicDependencyException>(() => n1.GetOrderedDependencies());
        }

        [Fact]
        public void ShortLoopShouldThrowCyclicDependencyException()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            
            n1.StackDependencies.Add(new DataDependency<int>(n2));
            n2.StackDependencies.Add(new DataDependency<int>(n1));

            Assert.Throws<CyclicDependencyException>(() => n1.GetOrderedDependencies());
        }

        [Fact]
        public void LongLoopShouldThrowCyclicDependencyException()
        {
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3, 3);
            var n4 = dfg.Nodes.Add(4, 4);
            
            n1.StackDependencies.Add(new DataDependency<int>(n2));
            n2.StackDependencies.Add(new DataDependency<int>(n3));
            n3.StackDependencies.Add(new DataDependency<int>(n4));
            n4.StackDependencies.Add(new DataDependency<int>(n1));

            Assert.Throws<CyclicDependencyException>(() => n1.GetOrderedDependencies());
        }

        [Fact]
        public void IgnoreVariableDependenciesWhenNotIncluded()
        {
            var variable = new DummyVariable("v1");
            
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3, 3);
            var n4 = dfg.Nodes.Add(4, 4);
            var n5 = dfg.Nodes.Add(5, 5);
            
            n1.StackDependencies.Add(new DataDependency<int>(n2));
            n1.VariableDependencies.Add(variable, new DataDependency<int>(n3));
            n2.StackDependencies.Add(new DataDependency<int>(n4));
            n2.VariableDependencies.Add(variable, new DataDependency<int>(n5));

            Assert.Equal(new[]
            {
                n4, n2, n1
            }, n1.GetOrderedDependencies(DependencyCollectionFlags.IncludeStackDependencies));
        }

        [Fact]
        public void IgnoreStackDependenciesWhenNotIncluded()
        {
            var variable = new DummyVariable("v1");
            
            var dfg = new DataFlowGraph<int>(IntArchitecture.Instance);
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            var n3 = dfg.Nodes.Add(3, 3);
            var n4 = dfg.Nodes.Add(4, 4);
            var n5 = dfg.Nodes.Add(5, 5);
            
            n1.StackDependencies.Add(new DataDependency<int>(n2));
            n1.VariableDependencies.Add(variable, new DataDependency<int>(n3));
            n3.StackDependencies.Add(new DataDependency<int>(n4));
            n3.VariableDependencies.Add(variable, new DataDependency<int>(n5));

            Assert.Equal(new[]
            {
                n5, n3, n1
            }, n1.GetOrderedDependencies(DependencyCollectionFlags.IncludeVariableDependencies));
        }
    }
}