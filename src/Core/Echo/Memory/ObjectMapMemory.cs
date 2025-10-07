using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Echo.Memory
{
    /// <summary>
    /// Provides a mechanism for mapping key objects to representative memory spaces.
    /// </summary>
    /// <typeparam name="TKey">The type of keys to map.</typeparam>
    /// <typeparam name="TSpace">The type of memory space to map each key to.</typeparam>
    public class ObjectMapMemory<TKey, TSpace> : IMemorySpace
        where TKey : notnull
        where TSpace : class, IMemorySpace
    {
        private readonly Dictionary<TKey, TSpace> _objectToSpace = new();
        private readonly Dictionary<long, TKey> _offsetToObject = new();
        private readonly VirtualMemory _backingBuffer;
        private readonly Func<TKey, TSpace> _factory;
        private long _currentOffset;

        /// <summary>
        /// Creates a new object map memory.
        /// </summary>
        /// <param name="size">The maximum size of the memory buffer.</param>
        /// <param name="factory">The function creating the memory space for the provided key.</param>
        public ObjectMapMemory(long size, Func<TKey, TSpace> factory)
        {
            _backingBuffer = new VirtualMemory(size);
            _factory = factory;
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
        /// Gets or creates the representative memory space for the provided key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The memory space.</returns>
        [return: NotNullIfNotNull(nameof(key))]
        public TSpace? GetOrCreate(TKey? key)
        {
            if (key is null)
                return null;

            if (!_objectToSpace.TryGetValue(key, out var space))
            {
                space = _factory(key);

                _backingBuffer.Map(_backingBuffer.AddressRange.Start + _currentOffset, space);
                _objectToSpace[key] = space;
                _offsetToObject[_currentOffset] = key;

                _currentOffset += space.AddressRange.Length;
            }

            return space;
        }

        /// <summary>
        /// Attempts to obtain the key associated to a provided address.
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="key">The key, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the key was found, <c>false</c> otherwise.</returns>
        public bool TryGetKey(long address, [NotNullWhen(true)] out TKey? key)
        {
            return _offsetToObject.TryGetValue(address - AddressRange.Start, out key);
        }

        /// <summary>
        /// Attempts to obtain the memory space associated to a provided address.
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="space">The space or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the space was found, <c>false</c> otherwise.</returns>
        public bool TryGetValue(long address, [NotNullWhen(true)] out TSpace? space)
        {
            space = null;
            return TryGetKey(address, out var key) && _objectToSpace.TryGetValue(key, out space);
        }

        /// <summary>
        /// Removes a key and its representative memory space from the map.
        /// </summary>
        /// <param name="key"></param>
        public void Unmap(TKey key)
        {
            if (!_objectToSpace.TryGetValue(key, out var space))
                return;

            _backingBuffer.Unmap(space.AddressRange.Start);
            _objectToSpace.Remove(key);
            _offsetToObject.Remove(space.AddressRange.Start);

            if (_offsetToObject.Count == 0)
                _currentOffset = 0;
        }

        /// <summary>
        /// Unmaps all registered keys and memory spaces from the memory.
        /// </summary>
        public void Clear()
        {
            foreach (var space in _objectToSpace.Values)
                _backingBuffer.Unmap(space.AddressRange.Start);

            _objectToSpace.Clear();
            _offsetToObject.Clear();
            _currentOffset = 0;
        }
    }

}