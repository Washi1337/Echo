using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// <summary>
        /// Memory spaces, sorted by their address range.
        /// </summary>
        private readonly List<IMemorySpace> _spaces = new();

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
            private set;
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

            int index = GetMemorySpaceIndex(address);
            if (index != -1)
                throw new ArgumentException($"Address {address:X8} is already in use.");

            // Insertion sort to ensure _spaces remains sorted.
            int i = 0;
            while (i < _spaces.Count && address >= _spaces[i].AddressRange.Start)
                i++;
            _spaces.Insert(i, space);

            // Assign proper address range to space.
            space.Rebase(address);
        }

        /// <summary>
        /// Unmaps a memory space that was mapped at the provided address.
        /// </summary>
        /// <param name="address">The address of the memory space to unmap.</param>
        public void Unmap(long address)
        {
            int index = GetMemorySpaceIndex(address);
            if (index != -1 && _spaces[index].AddressRange.Start == address)
                _spaces.RemoveAt(index);
        }

        /// <summary>
        /// Gets a collection of all ranges that were mapped into this virtual memory.
        /// </summary>
        /// <returns>The address ranges within the memory.</returns>
        public IEnumerable<AddressRange> GetMappedRanges() => _spaces.Select(s => s.AddressRange);

        private int GetMemorySpaceIndex(long address)
        {
            int left = 0;
            int right = _spaces.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var space = _spaces[mid];
                
                if (space.AddressRange.Contains(address))
                    return mid;
                if (address < space.AddressRange.Start)
                    right = mid - 1;
                else if (address >= space.AddressRange.End)
                    left = mid + 1;
            }

            return -1;
        }

        /// <inheritdoc />
        public bool IsValidAddress(long address)
        {
            int index = GetMemorySpaceIndex(address);
            return index != -1 && _spaces[index].IsValidAddress(address);
        }

        /// <inheritdoc />
        public void Rebase(long baseAddress)
        {
            foreach (var space in _spaces)
            {
                long relative = space.AddressRange.Start - AddressRange.Start;
                space.Rebase(baseAddress + relative);
            }
            
            AddressRange = new AddressRange(baseAddress, baseAddress + AddressRange.Length);
        }

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            if (buffer.Count == 0)
                return;

            int index = GetMemorySpaceIndex(address); 
            if (index == -1)
                throw new AccessViolationException();

            _spaces[index].Read(address, buffer);
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            if (buffer.Count == 0)
                return;
            
            int index = GetMemorySpaceIndex(address); 
            if (index == -1)
                throw new AccessViolationException();

            _spaces[index].Write(address, buffer);
        }

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length == 0)
                return;
            
            int index = GetMemorySpaceIndex(address); 
            if (index == -1)
                throw new AccessViolationException();

            _spaces[index].Write(address, buffer);
        }
    }
}

