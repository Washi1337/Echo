using System;
using System.Collections.Generic;

namespace Echo.Memory.Heap
{
    /// <summary>
    /// Provides a basic implementation of a heap.
    /// </summary>
    public class BasicHeap : IHeap
    {
        private readonly Dictionary<long, BitVector> _chunks = new();
        private readonly VirtualMemory _backingBuffer;
        private long _currentOffset = 0;
        
        /// <summary>
        /// Creates a new empty heap.
        /// </summary>
        /// <param name="size">The maximum size of the heap.</param>
        public BasicHeap(int size)
        {
            _backingBuffer = new VirtualMemory(size);
        }

        /// <inheritdoc />
        public AddressRange AddressRange => _backingBuffer.AddressRange;

        /// <inheritdoc />
        public long Allocate(uint size, bool initialize)
        {
            if (_currentOffset + size >= _backingBuffer.AddressRange.Length)
                throw new OutOfMemoryException();

            long address = _backingBuffer.AddressRange.Start + _currentOffset;
            _currentOffset = Align(_currentOffset + size, 8);

            var chunk = new BitVector((int) (size * 8), initialize);
            _backingBuffer.Map(address, new BasicMemorySpace(chunk));
            _chunks.Add(address, chunk);

            return address;
        }

        /// <inheritdoc />
        public void Free(long address)
        {
            if (_chunks.Remove(address))
                _backingBuffer.Unmap(address);
        }

        /// <inheritdoc />
        public uint GetChunkSize(long address) => (uint) (_chunks[address].Count / 8);

        /// <inheritdoc />
        public BitVectorSpan GetChunkSpan(long address) => _chunks[address].AsSpan();

        /// <inheritdoc />
        public IEnumerable<AddressRange> GetAllocatedChunks() => _backingBuffer.GetMappedRanges();

        /// <inheritdoc />
        public bool IsValidAddress(long address) => _backingBuffer.IsValidAddress(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _backingBuffer.Rebase(baseAddress);

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer) => _backingBuffer.Read(address, buffer);

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer) => _backingBuffer.Write(address, buffer);

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer) => _backingBuffer.Write(address, buffer);

        private static long Align(long value, long alignment)
        {
            alignment--;
            return (value + alignment) & ~alignment;
        }
    }
}