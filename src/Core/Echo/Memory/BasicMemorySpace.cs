using System;
using Echo.Code;

namespace Echo.Memory
{
    /// <summary>
    /// Provides a basic implementation of a <see cref="IMemorySpace"/>, where memory is one continuous block of data
    /// that is fully accessible.
    /// </summary>
    public class BasicMemorySpace : IMemorySpace
    {
        private long _baseAddress;
        
        /// <summary>
        /// Creates a new memory space.
        /// </summary>
        /// <param name="size">The number of bytes to store in the space.</param>
        /// <param name="initialize">Indicates whether the space should be initialized with zeroes.</param>
        public BasicMemorySpace(int size, bool initialize)
            : this(new BitVector(size * 8, initialize))
        {
        }
        
        /// <summary>
        /// Wraps a byte array into a memory space.
        /// </summary>
        /// <param name="backBuffer">The data of the memory space.</param>
        public BasicMemorySpace(byte[] backBuffer)
        {
            BackBuffer = new BitVector(backBuffer);
        }
        
        /// <summary>
        /// Wraps a bit vector into a memory space.
        /// </summary>
        /// <param name="backBuffer">The data of the memory space.</param>
        public BasicMemorySpace(BitVector backBuffer)
        {
            BackBuffer = backBuffer;
        }

        /// <summary>
        /// Gets the back buffer behind the memory space that stores the raw data.
        /// </summary>
        public BitVector BackBuffer
        {
            get;
        }

        /// <inheritdoc />
        public AddressRange AddressRange => new(_baseAddress, _baseAddress + BackBuffer.Count / 8);

        /// <inheritdoc />
        public bool IsValidAddress(long address) => AddressRange.Contains(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _baseAddress = baseAddress;

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            BackBuffer.AsSpan((int) (address - _baseAddress) * 8, buffer.Count).CopyTo(buffer);
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            buffer.CopyTo(BackBuffer.AsSpan((int) (address - _baseAddress) * 8, buffer.Count));
        }

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer)
        {
            BackBuffer.AsSpan((int) ((address - _baseAddress) * 8)).Write(buffer);
        }
    }
}