using Echo.DataFlow.Analysis;
using Xunit;

namespace Echo.DataFlow.Tests.Analysis
{
    public class DependencyCollectionTest
    {
        [Fact]
        public void NoDependencies()
        {
            var dfg = new DataFlowGraph<int>();
            var n1 = dfg.Nodes.Add(1, 1);

            Assert.Equal(new[]
            {
                n1
            }, n1.GetOrderedDependencies());
        }

        [Fact]
        public void SingleStackDependency()
        {
            var dfg = new DataFlowGraph<int>();
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
            var dfg = new DataFlowGraph<int>();
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
            var dfg = new DataFlowGraph<int>();
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
            var dfg = new DataFlowGraph<int>();
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
            var dfg = new DataFlowGraph<int>();
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
            var dfg = new DataFlowGraph<int>();
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
            var dfg = new DataFlowGraph<int>();
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
            var dfg = new DataFlowGraph<int>();
            var n1 = dfg.Nodes.Add(1, 1);
            n1.StackDependencies.Add(new DataDependency<int>(n1));

            Assert.Throws<CyclicDependencyException>(() => n1.GetOrderedDependencies());
        }

        [Fact]
        public void ShortLoopShouldThrowCyclicDependencyException()
        {
            var dfg = new DataFlowGraph<int>();
            var n1 = dfg.Nodes.Add(1, 1);
            var n2 = dfg.Nodes.Add(2, 2);
            
            n1.StackDependencies.Add(new DataDependency<int>(n2));
            n2.StackDependencies.Add(new DataDependency<int>(n1));

            Assert.Throws<CyclicDependencyException>(() => n1.GetOrderedDependencies());
        }

        [Fact]
        public void LongLoopShouldThrowCyclicDependencyException()
        {
            var dfg = new DataFlowGraph<int>();
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
    }
}