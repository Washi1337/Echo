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

        /// <inheritdoc />
        public AddressRange AddressRange => new(0, long.MaxValue);

        /// <summary>
        /// Maps a memory space at the provided virtual memory address.
        /// </summary>
        /// <param name="address">The address to map the data at.</param>
        /// <param name="space">The data to map.</param>
        /// <exception cref="ArgumentException">Occurs when the address was already in use.</exception>
        public void Map(long address, IMemorySpace space)
        {
            if (_mappings.ContainsKey(address))
                throw new ArgumentException($"Address {address:X8} is already in use.");

            _mappings.Add(address, space);
        }

        private bool TryFindMemoryMapping(long address, out MemoryMapping mapping)
        {
            mapping = _mappings.FirstOrDefault(x => x.Value.AddressRange.Contains(address - x.Key));
            return mapping.Key != 0;
        }

        /// <inheritdoc />
        public bool IsValidAddress(long address)
        {
            return TryFindMemoryMapping(address, out var space) && space.Value.IsValidAddress(address - space.Key);
        }

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            if (!TryFindMemoryMapping(address, out var space))
                throw new AccessViolationException();

            space.Value.Read(address - space.Key, buffer);
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            if (!TryFindMemoryMapping(address, out var space))
                throw new AccessViolationException();

            space.Value.Write(address - space.Key, buffer);
        }
    }
}

