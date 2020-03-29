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
    }
}