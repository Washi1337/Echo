using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Echo.Concrete
{
    public class BitVectorPool
    {
        private readonly ConcurrentDictionary<int, ConcurrentBag<BitVector>> _instancesBySize = new();

        public BitVector RentNativeInteger(bool is32Bit, bool initialize) => Rent(is32Bit ? 32 : 64, initialize);

        public BitVector Rent(int size, bool initialize)
        {
            var pool = GetInstancesOfSize(size);

            if (pool.TryTake(out var instance))
            {
                if (initialize)
                    instance.AsSpan().Clear();
                return instance;
            }

            return new BitVector(size, initialize);
        }

        public void Return(BitVector vector) => GetInstancesOfSize(vector.Count).Add(vector);

        private ConcurrentBag<BitVector> GetInstancesOfSize(int size)
        {
            if (!_instancesBySize.TryGetValue(size, out var pool))
            {
                pool = new ConcurrentBag<BitVector>();
                while (_instancesBySize.TryAdd(size, pool) || !_instancesBySize.TryGetValue(size, out pool))
                {
                    // ...
                }
            }

            return pool;
        }
    }
}