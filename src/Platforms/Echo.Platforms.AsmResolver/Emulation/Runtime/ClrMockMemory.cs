using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Runtime
{
    /// <summary>
    /// Provides a block of memory that contains mock data modelling CLR specific data structures.
    /// </summary>
    public class ClrMockMemory : IMemorySpace
    {
        private readonly VirtualMemory _backingBuffer;

        /// <summary>
        /// Creates a new instance of the <see cref="ClrMockMemory"/> block.
        /// </summary>
        public ClrMockMemory()
        {
            _backingBuffer = new VirtualMemory(0x100_0000);
            var comparer = new SignatureComparer();
            _backingBuffer.Map(0, MethodTables = new GenericMockMemory<ITypeDescriptor>(0x100_0000, 0x100, comparer));
        }
        
        /// <inheritdoc />
        public AddressRange AddressRange => _backingBuffer.AddressRange;

        /// <summary>
        /// Gets the memory assigned for method table structures (types).
        /// </summary>
        public GenericMockMemory<ITypeDescriptor> MethodTables
        {
            get;
        }

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
    }
}