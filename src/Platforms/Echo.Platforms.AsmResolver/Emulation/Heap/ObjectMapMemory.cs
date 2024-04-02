using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Heap
{
    /// <summary>
    /// Provides a memory space for hosting managed objects within memory of a virtual machine. 
    /// </summary>
    public class ObjectMapMemory : IMemorySpace
    {
        private readonly Dictionary<object, MappedObject> _mappedObjects = new();
        private readonly Dictionary<long, MappedObject> _objectsByOffset = new();
        private readonly VirtualMemory _backingBuffer;
        private readonly CilVirtualMachine _machine;
        private long _currentOffset;

        /// <summary>
        /// Creates a new object mapping host memory space.
        /// </summary>
        /// <param name="machine">The machine the memory is associated with.</param>
        /// <param name="size">The maximum size of the memory.</param>
        public ObjectMapMemory(CilVirtualMachine machine, long size)
        {
            _machine = machine;
            _backingBuffer = new VirtualMemory(size);
        }

        /// <inheritdoc />
        public AddressRange AddressRange => _backingBuffer.AddressRange;

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

        /// <summary>
        /// Gets or creates a new address mapping for the provided object.
        /// </summary>
        /// <param name="value">The object to embed into the machine.</param>
        /// <returns>The mapped object.</returns>
        [return: NotNullIfNotNull("value")]
        public MappedObject? GetOrCreateMapping(object? value)
        {
            if (value is null)
                return null;
            
            if (!_mappedObjects.TryGetValue(value, out var map))
            {
                map = new MappedObject(value, GetLayout(value), _machine);
                
                _backingBuffer.Map(_backingBuffer.AddressRange.Start + _currentOffset, map);
                _mappedObjects[value] = map;
                _objectsByOffset[_currentOffset] = map;
                
                _currentOffset += map.AddressRange.Length;
            }

            return map;
        }

        /// <summary>
        /// Attempts to obtain the managed object mapping by its address.
        /// </summary>
        /// <param name="address">The address of the mapped object.</param>
        /// <param name="map">The obtained mapped object.</param>
        /// <returns><c>true</c> if the object was found, <c>false</c> otherwise.</returns>
        public bool TryGetObject(long address, [NotNullWhen(true)] out MappedObject? map)
        {
            return _objectsByOffset.TryGetValue(address - AddressRange.Start, out map);
        }

        private TypeMemoryLayout GetLayout(object value)
        {
            var type = value.GetType();
            var descriptor = _machine.ContextModule.DefaultImporter.ImportType(type);

            // Special treatment for array types.
            if (descriptor is TypeSpecification { Signature: SzArrayTypeSignature arrayType })
            {
                uint totalSize = _machine.ValueFactory.TypeManager.GetArrayObjectSize(arrayType.BaseType, ((Array) value).Length);
                uint dataSize = totalSize - _machine.ValueFactory.TypeManager.ObjectHeaderSize;
                return new TypeMemoryLayout(
                    descriptor, 
                    dataSize,
                    _machine.Is32Bit ? MemoryLayoutAttributes.Is32Bit : MemoryLayoutAttributes.Is64Bit);
            }

            return _machine.ValueFactory.TypeManager.GetMethodTable(descriptor).ContentsLayout;
        }

        /// <summary>
        /// Unmaps a registered object from the host memory.
        /// </summary>
        /// <param name="value">The object to unmap.</param>
        public void Unmap(object value)
        {
            if (!_mappedObjects.TryGetValue(value, out var mapping))
                return;
            
            _backingBuffer.Unmap(mapping.AddressRange.Start);
            _mappedObjects.Remove(value);
            _objectsByOffset.Remove(mapping.AddressRange.Start);

            if (_objectsByOffset.Count == 0)
                _currentOffset = 0;
        }

        /// <summary>
        /// Unmaps all registered objects from the host memory.
        /// </summary>
        public void Clear()
        {
            foreach (var map in _objectsByOffset.Values)
                _backingBuffer.Unmap(map.AddressRange.Start);

            _mappedObjects.Clear();
            _objectsByOffset.Clear();
            _currentOffset = 0;
        }
    }

}