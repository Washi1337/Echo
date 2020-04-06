using System;
using Echo.ControlFlow.Regions;
using Echo.Platforms.DummyPlatform;
using Xunit;

namespace Echo.ControlFlow.Tests.Regions
{
    public class RegionCollectionTest
    {
        [Fact]
        public void AddSubRegionShouldSetParentRegion()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var region = new BasicControlFlowRegion<int>();
            
            Assert.Null(region.ParentRegion);
            graph.Regions.Add(region);
            Assert.Same(graph, region.ParentRegion);
        }

        [Fact]
        public void RemoveSubRegionShouldUnsetParentRegion()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var region = new BasicControlFlowRegion<int>();
            
            graph.Regions.Add(region);
            Assert.Same(graph, region.ParentRegion);
            graph.Regions.Remove(region);
            Assert.Null(region.ParentRegion);
        }

        [Fact]
        public void ClearAllSubRegionsShouldUnsetAllParents()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);

            var regions = new[]
            {
                new BasicControlFlowRegion<int>(), new BasicControlFlowRegion<int>(), new BasicControlFlowRegion<int>(),
            };

            foreach (var region in regions)
                graph.Regions.Add(region);
            
            Assert.All(regions, r => Assert.Same(graph, r.ParentRegion));
            
            graph.Regions.Clear();
            
            Assert.All(regions, r => Assert.Null(r.ParentRegion)); 
        }

        [Fact]
        public void AddingRegionSecondTimeShouldThrow()
        { 
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var region = new BasicControlFlowRegion<int>();

            graph.Regions.Add(region);

            Assert.Throws<ArgumentException>(() => graph.Regions.Add(region));
        }

        [Fact]
        public void AddingRegionOfAnotherGraphShouldThrow()
        { 
            var graph1 = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var graph2 = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var region = new BasicControlFlowRegion<int>();

            graph1.Regions.Add(region);

            Assert.Throws<ArgumentException>(() => graph2.Regions.Add(region));
        }

        [Fact]
        public void AddNodeNotAddedToGraphToRegionShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0);
            
            var region = new BasicControlFlowRegion<int>();
            graph.Regions.Add(region);

            Assert.Throws<ArgumentException>(() => region.Nodes.Add(node));
        }

        [Fact]
        public void AddNodeToNotAddedRegionShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0);
            graph.Nodes.Add(node);
            
            var region = new BasicControlFlowRegion<int>();

            Assert.Throws<InvalidOperationException>(() => region.Nodes.Add(node));
        }

        [Fact]
        public void AddNodeToRegionShouldSetParentRegion()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0);
            graph.Nodes.Add(node);
            
            var region = new BasicControlFlowRegion<int>();
            graph.Regions.Add(region);

            region.Nodes.Add(node);

            Assert.Same(region, node.ParentRegion);
        }

        [Fact]
        public void RemoveNodeShouldSetRegionBackToParentGraph()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0);
            graph.Nodes.Add(node);
            
            var region = new BasicControlFlowRegion<int>();
            graph.Regions.Add(region);

            region.Nodes.Add(node);
            region.Nodes.Remove(node);
            
            Assert.Same(graph, node.ParentRegion);
        }

        [Fact]
        public void AddNodeFromAnotherRegionShouldThrow()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0);
            graph.Nodes.Add(node);
            
            var region1 = new BasicControlFlowRegion<int>();
            var region2 = new BasicControlFlowRegion<int>();
            graph.Regions.Add(region1);
            graph.Regions.Add(region2);

            region1.Nodes.Add(node);

            Assert.Throws<ArgumentException>(() => region2.Nodes.Add(node));
        }

        [Fact]
        public void RemoveNodeInRegionShouldRemoveFromRegion()
        {
            var graph = new ControlFlowGraph<int>(IntArchitecture.Instance);
            var node = new ControlFlowNode<int>(0);
            graph.Nodes.Add(node);
            
            var region = new BasicControlFlowRegion<int>();
            graph.Regions.Add(region);
            region.Nodes.Add(node);

            graph.Nodes.Remove(node);
            
            Assert.Null(node.ParentRegion);
            Assert.DoesNotContain(node, region.Nodes);
        }
    }
}