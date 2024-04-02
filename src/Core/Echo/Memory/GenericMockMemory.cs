using System.Collections.Generic;

namespace Echo.Memory
{
    /// <summary>
    /// Provides a semi high-level mapping between addresses and objects that live outside of the sandbox. 
    /// </summary>
    /// <typeparam name="T">The type of objects to store.</typeparam>
    /// <remarks>
    /// Reading from this memory chunk always results in reading unknown memory.
    /// </remarks>
    public class GenericMockMemory<T> : UninitializedMemorySpace
    {
        private readonly Dictionary<T, long> _itemToOffset;
        private readonly Dictionary<long, T> _offsetToItem;
        private readonly int _itemSize;
        private long _currentOffset = 0;

        /// <summary>
        /// Creates a new generic mock memory chunk.
        /// </summary>
        /// <param name="size">The size in bytes of the memory.</param>
        /// <param name="itemSize">The size in bytes of a single element.</param>
        public GenericMockMemory(int size, int itemSize)
            : this(size, itemSize, EqualityComparer<T>.Default)
        {
        }
        
        /// <summary>
        /// Creates a new generic mock memory chunk.
        /// </summary>
        /// <param name="size">The size in bytes of the memory.</param>
        /// <param name="itemSize">The size in bytes of a single element.</param>
        /// <param name="comparer">The equality comparer to use for comparing elements for uniqueness.</param>
        public GenericMockMemory(int size, int itemSize, IEqualityComparer<T> comparer)
            : base(size)
        {
            _itemSize = itemSize;
            _itemToOffset = new Dictionary<T, long>(comparer);
            _offsetToItem = new Dictionary<long, T>();
        }

        /// <summary>
        /// Gets the address or assigns a new address to the provided object. 
        /// </summary>
        /// <param name="item">The object.</param>
        /// <returns>The address.</returns>
        public long GetAddress(T item)
        {
            if (!_itemToOffset.TryGetValue(item, out long offset))
            {
                offset = _currentOffset;
                _itemToOffset.Add(item, offset);
                _offsetToItem.Add(offset, item);
                _currentOffset += _itemSize;
            }

            return AddressRange.Start + offset;
        }

        /// <summary>
        /// Attempts to get the object at the provided address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="value">The object.</param>
        /// <returns><c>true</c> if the address maps to an object, <c>false</c> otherwise.</returns>
        public bool TryGetObject(long address, out T? value)
        {
            return _offsetToItem.TryGetValue(address - AddressRange.Start, out value);
        }

        /// <summary>
        /// Gets the object at the provided address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The object.</returns>
        public T GetObject(long address) => _offsetToItem[address - AddressRange.Start];
    }
}