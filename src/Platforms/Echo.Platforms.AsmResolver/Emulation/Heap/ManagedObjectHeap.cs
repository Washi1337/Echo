using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Memory;
using Echo.Memory.Heap;
using Echo.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Heap
{
    /// <summary>
    /// Represents a heap of managed objects.
    /// </summary>
    public class ManagedObjectHeap : IMemorySpace
    {
        private readonly Dictionary<string, long> _internedStrings = new();
        private readonly IHeap _backingHeap;
        private readonly ValueFactory _factory;

        /// <summary>
        /// Creates a new empty heap for managed objects.
        /// </summary>
        /// <param name="size">The maximum size the heap can grow.</param>
        /// <param name="factory">The object responsible for managing type information.</param>
        public ManagedObjectHeap(int size, ValueFactory factory)
            : this(new BasicHeap(size), factory)
        {
        }

        /// <summary>
        /// Creates a new empty heap for managed objects.
        /// </summary>
        /// <param name="backingHeap">The backing heap to use for allocating raw memory.</param>
        /// <param name="factory">The object responsible for managing type information.</param>
        public ManagedObjectHeap(IHeap backingHeap, ValueFactory factory)
        {
            _backingHeap = backingHeap;
            _factory = factory;
        }

        /// <inheritdoc />
        public AddressRange AddressRange => _backingHeap.AddressRange;

        /// <summary>
        /// Allocates a managed object of the provided type in the heap.
        /// </summary>
        /// <param name="type">The type of object to allocate.</param>
        /// <param name="initialize">A value indicating whether the object should be initialized with zeroes.</param>
        /// <returns>The address of the object that was allocated.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the provided type's size is unknown (e.g. when the type is an array or string type).
        /// </exception>
        public long AllocateObject(ITypeDescriptor type, bool initialize)
        {
            switch (type.ToTypeSignature().ElementType)
            {
                case ElementType.Array:
                case ElementType.SzArray:
                    throw new InvalidOperationException("Cannot allocate an array without knowing its size.");

                case ElementType.String:
                    throw new InvalidOperationException("Cannot allocate a string without knowing its size.");
            }
            
            long address = _backingHeap.Allocate(_factory.GetObjectSize(type), initialize);
            var chunkSpan = _backingHeap.GetChunkSpan(address);
            SetMethodTable(chunkSpan, type);
            return address;
        }

        /// <summary>
        /// Allocates a single dimension, zero-based array on the heap.
        /// </summary>
        /// <param name="elementType">The type of elements the array stores.</param>
        /// <param name="elementCount">The number of elements in the array.</param>
        /// <param name="initialize">A value indicating whether the elements should be initialized with zeroes.</param>
        /// <returns>The address of the array that was allocated.</returns>
        public long AllocateSzArray(TypeSignature elementType, int elementCount, bool initialize)
        {
            // Allocate chunk of memory for object.
            long address = _backingHeap.Allocate(_factory.GetArrayObjectSize(elementType, elementCount), initialize);
            var chunkSpan = _backingHeap.GetChunkSpan(address);

            // Set object type.
            SetMethodTable(chunkSpan, elementType.MakeSzArrayType());

            // Set array length field.
            chunkSpan.SliceArrayLength(_factory).WriteNativeInteger(elementCount, _factory.Is32Bit);
            
            return address;
        }

        /// <summary>
        /// Allocates a string into the virtual managed heap.
        /// </summary>
        /// <param name="value">The string literal.</param> 
        /// <returns>The address of the string that was allocated.</returns>
        public long AllocateString(string value)
        {
            long address = AllocateString(value.Length, false);
          
            if (value.Length > 0)
            {
                var dataSpan = _backingHeap.GetChunkSpan(address).SliceStringData(_factory);
                dataSpan.Write(MemoryMarshal.Cast<char, byte>(value.AsSpan()));
            }

            return address;
        }
        
        /// <summary>
        /// Allocates a string into the virtual managed heap.
        /// </summary>
        /// <param name="length">The number of characters in the string.</param>
        /// <param name="initialize">A value indicating whether all characters should be initialized with zeroes.</param>
        /// <returns>The address of the string that was allocated.</returns>
        public long AllocateString(int length, bool initialize)
        {
            long address = _backingHeap.Allocate(_factory.GetStringObjectSize(length), initialize);
            var chunkSpan = _backingHeap.GetChunkSpan(address);

            // Set object type.
            SetMethodTable(chunkSpan, _factory.ContextModule.CorLibTypeFactory.String);
            
            // Set string length field.
            chunkSpan.SliceStringLength(_factory).Write(length);

            return address;
        }

        /// <summary>
        /// Gets the address to an interned string in the virtual managed heap.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <returns>The address of the interned string.</returns>
        public long GetInternedString(string value)
        {
            if (!_internedStrings.TryGetValue(value, out long address))
            {
                address = AllocateString(value);
                _internedStrings.Add(value, address);
            }

            return address;
        }
        
        private void SetMethodTable(BitVectorSpan objectSpan, ITypeDescriptor type) => objectSpan
            .SliceObjectMethodTable(_factory)
            .WriteNativeInteger(_factory.ClrMockMemory.MethodTables.GetAddress(type), _factory.Is32Bit);

        /// <summary>
        /// Gets the size of the object at the provided address.
        /// </summary>
        /// <param name="address">The address of the object.</param>
        /// <returns>The size in bytes.</returns>
        public uint GetObjectSize(long address) => _backingHeap.GetChunkSize(address);

        /// <summary>
        /// Gets a bit vector slice that spans the contents of the object at the provided address. 
        /// </summary>
        /// <param name="address">The address of the object.</param>
        /// <returns>The object slice.</returns>
        public BitVectorSpan GetObjectSpan(BitVectorSpan address)
        {
            if (!address.IsFullyKnown)
                throw new ArgumentException("Provided address is not fully known.");
            if (address.ByteCount != _factory.PointerSize)
                throw new ArgumentException("Provided address does not have the size of a pointer.");

            return GetObjectSpan(address.ReadNativeInteger(_factory.Is32Bit));
        }

        /// <summary>
        /// Gets a bit vector slice that spans the contents of the object at the provided address. 
        /// </summary>
        /// <param name="address">The address of the object.</param>
        /// <returns>The object slice.</returns>
        public BitVectorSpan GetObjectSpan(long address) => _backingHeap.GetChunkSpan(address);
        
        /// <summary>
        /// Obtains a collection of address ranges of all managed objects currently in the heap.
        /// </summary>
        /// <returns>The ranges.</returns>
        public IEnumerable<AddressRange> GetObjectRanges() => _backingHeap.GetAllocatedChunks();
        
        /// <summary>
        /// Releases an object from the heap.
        /// </summary>
        /// <param name="address">The address of the object.</param>
        public void Free(long address) => _backingHeap.Free(address);
        
        /// <inheritdoc />
        public bool IsValidAddress(long address) => _backingHeap.IsValidAddress(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _backingHeap.Rebase(baseAddress);

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer) => _backingHeap.Read(address, buffer);

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer) => _backingHeap.Write(address, buffer);
        
        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer) => _backingHeap.Write(address, buffer);
    }
}