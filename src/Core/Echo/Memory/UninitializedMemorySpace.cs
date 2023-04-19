using System;

namespace Echo.Memory
{
    /// <summary>
    /// Represents a chunk of uninitialized (unknown) memory. Writing to this memory space does not change the contents.
    /// </summary>
    public class UninitializedMemorySpace : IMemorySpace
    {
        /// <summary>
        /// Creates a new uninitialized memory space.
        /// </summary>
        /// <param name="size">The number of bytes to store in the space.</param>
        public UninitializedMemorySpace(int size)
        {
            AddressRange = new AddressRange(0, size);
        }

        /// <inheritdoc />
        public AddressRange AddressRange
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool IsValidAddress(long address) => AddressRange.Contains(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => AddressRange = new AddressRange(baseAddress, baseAddress + AddressRange.Length);

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer) => buffer.MarkFullyUnknown();

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
        }

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer)
        {
        }
    }
}