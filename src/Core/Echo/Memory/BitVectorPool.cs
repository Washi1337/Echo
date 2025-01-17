using System.Collections.Concurrent;

namespace Echo.Memory
{
    /// <summary>
    /// Provides a mechanism for reusing instances of <see cref="BitVector"/>.
    /// </summary>
    public class BitVectorPool
    {
        private readonly ConcurrentDictionary<int, ConcurrentBag<BitVector>> _freeInstancesBySize = new();
        private readonly ConcurrentDictionary<BitVector, byte> _instances = new();

        /// <summary>
        /// Rents a single bit vector of the provided size.
        /// </summary>
        /// <param name="size">The number of bits in the vector to rent.</param>
        /// <param name="initialize">A value indicating whether the bits should be cleared out or marked unknown.</param>
        /// <returns>The bit vector.</returns>
        public BitVector Rent(int size, bool initialize)
        {
            var pool = GetInstancesOfSize(size);

            if (pool.TryTake(out var instance))
            {
                if (initialize)
                    instance.AsSpan().Clear();
                else 
                    instance.AsSpan().MarkFullyUnknown();
                
                return instance;
            }

            instance = new BitVector(size, initialize);
            while (!_instances.TryAdd(instance, 0))
            {
                // Repeat until we succeed.
            }

            return instance;
        }

        /// <summary>
        /// Rents a native integer bit vector.
        /// </summary>
        /// <param name="is32Bit">A value indicating the vector should be 32 or 64 bits long.</param>
        /// <param name="initialize">A value indicating whether the bits should be cleared out or marked unknown.</param>
        /// <returns>The bit vector.</returns>
        public BitVector RentNativeInteger(bool is32Bit, bool initialize) => Rent(is32Bit ? 32 : 64, initialize);

        /// <summary>
        /// Returns the bit vector to the pool.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns><c>true</c> if the bitvector was an instance of the pool, <c>false</c> otherwise.</returns>
        public bool Return(BitVector? vector)
        {
            if (vector is null)
                return false;

            if (_instances.ContainsKey(vector))
            {
                GetInstancesOfSize(vector.Count).Add(vector);
                return true;
            }

            return false;
        }

        private ConcurrentBag<BitVector> GetInstancesOfSize(int size)
        {
            if (!_freeInstancesBySize.TryGetValue(size, out var pool))
            {
                pool = new ConcurrentBag<BitVector>();
                while (_freeInstancesBySize.TryAdd(size, pool))
                {
                    if (_freeInstancesBySize.TryGetValue(size, out var newPool))
                        pool = newPool;
                }
            }

            return pool;
        }
    }
}