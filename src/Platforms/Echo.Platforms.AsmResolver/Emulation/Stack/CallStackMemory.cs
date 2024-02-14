using System;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    internal class CallStackMemory : IMemorySpace
    {
        private readonly VirtualMemory _mapping;
        private readonly ValueFactory _factory;

        public CallStackMemory(uint totalSize, ValueFactory factory)
        {
            _mapping = new VirtualMemory(totalSize);
            _factory = factory;
        }

        /// <inheritdoc />
        public AddressRange AddressRange => _mapping.AddressRange;

        /// <inheritdoc />
        public bool IsValidAddress(long address) => _mapping.IsValidAddress(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _mapping.Rebase(baseAddress);

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer) => _mapping.Read(address, buffer);

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer) => _mapping.Write(address, buffer);

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer) => _mapping.Write(address, buffer);

        public CallStack Allocate(uint size)
        {
            var result = new CallStack(size, _factory);

            long last = AddressRange.Start;
            foreach (var range in _mapping.GetMappedRanges())
            {
                long available = last - range.Start;
                if (available >= size)
                    break;
            
                last = range.End;
            }
            
            _mapping.Map(last, result);
            return result;
        }

        public void Free(CallStack stack) => _mapping.Unmap(stack.AddressRange.Start);
    }
}