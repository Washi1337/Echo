using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Concrete.Memory.Heap
{
    /// <summary>
    /// Provides members for allocating and freeing chunks of memory.
    /// </summary>
    public interface IHeap : IMemorySpace
    {
        /// <summary>
        /// Allocates a chunk of uninitialized memory in the heap.
        /// </summary>
        /// <param name="size">The size of the chunk in bytes.</param>
        /// <param name="initialize">A value indicating whether the chunk of memory should be cleared out with zeroes.</param>
        /// <returns>The address of the allocated chunk.</returns>
        long Allocate(uint size, bool initialize);

        /// <summary>
        /// Releases a chunk of memory in the heap.
        /// </summary>
        /// <param name="address">The address of the chunk to free.</param>
        void Free(long address);

        /// <summary>
        /// Gets the size of the chunk that was allocated at the provided address.
        /// </summary>
        /// <param name="address">The address of the chunk.</param>
        /// <returns>The size in bytes.</returns>
        uint GetChunkSize(long address);

        /// <summary>
        /// Obtains a writable bit vector slice that spans the entire chunk at a provided address.
        /// </summary>
        /// <param name="address">The address of the chunk</param>
        /// <returns>The chunk slice.</returns>
        BitVectorSpan GetChunkSpan(long address);

        /// <summary>
        /// Gets a collection of all chunk address ranges within the heap that are currently allocated. 
        /// </summary>
        /// <returns>The ranges.</returns>
        IEnumerable<AddressRange> GetAllocatedChunks();
    }
}