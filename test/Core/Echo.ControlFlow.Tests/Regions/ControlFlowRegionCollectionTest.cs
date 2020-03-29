using System;
using Echo.ControlFlow.Regions;
using Echo.Platforms.DummyPlatform;
using Xunit;

namespace Echo.ControlFlow.Tests.Regions
{
    public class ControlFlowRegionCollectionTest
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
    }
}