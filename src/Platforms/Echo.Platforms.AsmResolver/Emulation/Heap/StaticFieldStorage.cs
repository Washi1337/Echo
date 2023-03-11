using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Concrete.Memory.Heap;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Heap
{
    /// <summary>
    /// Represents a chunk of memory designated for storing static fields in a .NET process.
    /// </summary>
    public class StaticFieldStorage : IMemorySpace
    {
        private readonly Dictionary<IFieldDescriptor, long> _fields = new(SignatureComparer.Default);
        private readonly ValueFactory _valueFactory;
        private readonly IHeap _heap;

        /// <summary>
        /// Creates a new empty static field storage.
        /// </summary>
        /// <param name="valueFactory">The value factory responsible for measuring and constructing new values.</param>
        /// <param name="size">The maximum size of the storage.</param>
        public StaticFieldStorage(ValueFactory valueFactory, int size)
            : this(valueFactory, new BasicHeap(size))
        {
        }

        /// <summary>
        /// Creates a new empty static field storage.
        /// </summary>
        /// <param name="valueFactory">The value factory responsible for measuring and constructing new values.</param>
        /// <param name="heap">The virtual heap to use for storing the field values.</param>
        public StaticFieldStorage(ValueFactory valueFactory, IHeap heap)
        {
            _valueFactory = valueFactory;
            _heap = heap;
        }

        /// <inheritdoc />
        public AddressRange AddressRange => _heap.AddressRange;

        /// <summary>
        /// Gets the address of the provided field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The address.</returns>
        public long GetFieldAddress(IFieldDescriptor field)
        {
            if (!_fields.TryGetValue(field, out long address))
            {
                var layout = _valueFactory.GetTypeValueMemoryLayout(field.Signature!.FieldType);
                address = _heap.Allocate(layout.Size, true);
                _fields.Add(field, address);
            }

            return address;
        }

        /// <summary>
        /// Obtains a writable bitvector slice that spans the value of the provided field. 
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The slice.</returns>
        public BitVectorSpan GetFieldSpan(IFieldDescriptor field) => _heap.GetChunkSpan(GetFieldAddress(field));

        /// <inheritdoc />
        public bool IsValidAddress(long address) => _heap.IsValidAddress(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _heap.Rebase(baseAddress);

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer) => _heap.Read(address, buffer);

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer) => _heap.Write(address, buffer);

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer) => _heap.Write(address, buffer);
    }
}