using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Memory;
using Echo.Code;

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
            _backingBuffer = new VirtualMemory(0x0300_0000);

            MethodTables = new GenericMockMemory<ITypeDescriptor>(0x0100_0000, 0x100, SignatureComparer.Default);
            Methods = new GenericMockMemory<IMethodDescriptor>(0x0100_0000, 0x20, SignatureComparer.Default);
            Fields = new GenericMockMemory<IFieldDescriptor>(0x0100_0000, 0x20, SignatureComparer.Default);

            _backingBuffer.Map(0x0000_0000, MethodTables);
            _backingBuffer.Map(0x0100_0000, Methods);
            _backingBuffer.Map(0x0200_0000, Fields);
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

        /// <summary>
        /// Gets the memory assigned for method descriptor structures.
        /// </summary>
        public GenericMockMemory<IMethodDescriptor> Methods
        {
            get;
        }

        /// <summary>
        /// Gets the memory assigned for field descriptor structures.
        /// </summary>
        public GenericMockMemory<IFieldDescriptor> Fields
        {
            get;
        }

        /// <inheritdoc />
        public bool IsValidAddress(long address) => _backingBuffer.IsValidAddress(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _backingBuffer.Rebase(baseAddress);

        /// <inheritdoc />
        public void Read(long address, Memory.BitVectorSpan buffer) => _backingBuffer.Read(address, buffer);

        /// <inheritdoc />
        public void Write(long address, Memory.BitVectorSpan buffer) => _backingBuffer.Write(address, buffer);

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer) => _backingBuffer.Write(address, buffer);
    }
}