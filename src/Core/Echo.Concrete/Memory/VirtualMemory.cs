using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

using MemoryMapping = System.Collections.Generic.KeyValuePair<long, Echo.Concrete.Memory.IMemorySpace>;

namespace Echo.Concrete.Memory
{
    /// <summary>
    /// Represents an addressable region of memory that maps a collection of memory spaces to virtual addresses.
    /// </summary>
    /// <remarks>
    /// This class can be compared to the entire memory space of a running process.
    /// </remarks>
    public class VirtualMemory : IMemorySpace
    {
        private readonly Dictionary<long, IMemorySpace> _mappings = new();

        /// <summary>
        /// Creates new uninitialized virtual memory.
        /// </summary>
        public VirtualMemory()
            : this(long.MaxValue)
        {
        }

        /// <summary>
        /// Creates new uninitialized virtual memory with the provided size.
        /// </summary>
        public VirtualMemory(long size)
        {
            AddressRange = new AddressRange(0, size);
        }

        /// <inheritdoc />
        public AddressRange AddressRange
        {
            get;
        }

        /// <summary>
        /// Maps a memory space at the provided virtual memory address.
        /// </summary>
        /// <param name="address">The address to map the data at.</param>
        /// <param name="space">The data to map.</param>
        /// <exception cref="ArgumentException">Occurs when the address was already in use.</exception>
        public void Map(long address, IMemorySpace space)
        {
            if (!AddressRange.Contains(address))
                throw new ArgumentException($"Address {address:X8} does not fall within the virtual memory.");

            if (_mappings.ContainsKey(address))
                throw new ArgumentException($"Address {address:X8} is already in use.");

            _mappings.Add(address, space);
        }

        /// <summary>
        /// Unmaps a memory space that was mapped at the provided address.
        /// </summary>
        /// <param name="address">The address of the memory space to unmap.</param>
        public void Unmap(long address) => _mappings.Remove(address);

        /// <summary>
        /// Gets a collection of all ranges that were mapped into this virtual memory.
        /// </summary>
        /// <returns>The address ranges within the memory.</returns>
        public IEnumerable<AddressRange> GetMappedRanges() => _mappings.Select(item =>
            new AddressRange(item.Key, item.Key + item.Value.AddressRange.Length));

        private bool TryFindMemoryMapping(long address, out MemoryMapping mapping)
        {
            foreach (var m in _mappings)
            {
                if (m.Value.AddressRange.Contains(address - m.Key))
                {
                    mapping = m;
                    return true;
                }
            }

            mapping = default;
            return false;
        }

        /// <inheritdoc />
        public bool IsValidAddress(long address)
        {
            return TryFindMemoryMapping(address, out var space) && space.Value.IsValidAddress(address - space.Key);
        }

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            if (buffer.Count == 0)
                return;
            if (!TryFindMemoryMapping(address, out var space))
                throw new AccessViolationException();

            space.Value.Read(address - space.Key, buffer);
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            if (buffer.Count == 0)
                return;
            if (!TryFindMemoryMapping(address, out var space))
                throw new AccessViolationException();

            space.Value.Write(address - space.Key, buffer);
        }

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer)
        {
            if (!TryFindMemoryMapping(address, out var space))
                throw new AccessViolationException();

            space.Value.Write(address - space.Key, buffer);
        }
    }
}

