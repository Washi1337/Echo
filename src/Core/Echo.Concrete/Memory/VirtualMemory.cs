using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

using MemoryMapping = System.Collections.Generic.KeyValuePair<long, Echo.Concrete.Memory.IMemorySpace>;

namespace Echo.Concrete.Memory
{
    public class VirtualMemory : IMemorySpace
    {
        private readonly Dictionary<long, IMemorySpace> _mappings = new();

        public AddressRange AddressRange => new(0, long.MaxValue);

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

        public bool IsValidAddress(long address)
        {
            return TryFindMemoryMapping(address, out var space) && space.Value.IsValidAddress(address - space.Key);
        }

        public void Read(long address, BitVectorSpan buffer)
        {
            if (!TryFindMemoryMapping(address, out var space))
                throw new AccessViolationException();

            space.Value.Read(address - space.Key, buffer);
        }

        public void Write(long address, BitVectorSpan buffer)
        {
            if (!TryFindMemoryMapping(address, out var space))
                throw new AccessViolationException();

            space.Value.Write(address - space.Key, buffer);
        }
    }
}