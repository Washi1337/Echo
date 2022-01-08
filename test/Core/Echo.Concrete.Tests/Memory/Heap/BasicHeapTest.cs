using Echo.Concrete.Memory.Heap;
using Xunit;

namespace Echo.Concrete.Tests.Memory.Heap
{
    public class BasicHeapTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData(0x0040_0000)]
        public void AllocateShouldResultInAddressWithinHeap(long baseAddress)
        {
            var heap = new BasicHeap(0x1000);
            heap.Rebase(baseAddress);

            long address = heap.Allocate(0x100, false);
            Assert.True(heap.AddressRange.Contains(address));
            Assert.True(heap.IsValidAddress(address));
            var chunkRange = Assert.Single(heap.GetAllocatedChunks());
            Assert.Equal(address, chunkRange.Start);
        }
        
        [Theory]
        [InlineData(0x0040_0000)]
        public void RebaseShouldUpdateRanges(long baseAddress)
        {
            var heap = new BasicHeap(0x1000);

            heap.Allocate(0x100, false);

            var chunkRange = Assert.Single(heap.GetAllocatedChunks());

            heap.Rebase(baseAddress);
            
            var newChunkRange = Assert.Single(heap.GetAllocatedChunks());
            Assert.NotEqual(chunkRange, newChunkRange);
            Assert.True(heap.AddressRange.Contains(newChunkRange.Start));
        }
    }
}