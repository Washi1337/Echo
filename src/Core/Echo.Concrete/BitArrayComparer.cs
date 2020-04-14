using System;
using System.Collections;
using System.Collections.Generic;
using Echo.Concrete.Extensions;

namespace Echo.Concrete
{
    /// <summary>
    /// Represents a comparer that can compare bit arrays by their contents.
    /// </summary>
    public class BitArrayComparer : EqualityComparer<BitArray>
    {
        /// <summary>
        /// Gets a reusable instance of the <see cref="BitArrayComparer"/> class.
        /// </summary>
        public static BitArrayComparer Instance
        {
            get;
        } = new BitArrayComparer();

        /// <inheritdoc />
        public override bool Equals(BitArray x, BitArray y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null || x.Count != y.Count)
                return false;

            return x.AsSpan().SequenceEqual(y.AsSpan());
        }

        /// <inheritdoc />
        public override int GetHashCode(BitArray obj)
        {
            if (obj is null)
                return 0;

            var raw = obj.AsSpan();
            
            int hashCode = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                unchecked
                {
                    hashCode = (hashCode * 397) ^ raw[i];
                }
            }

            return hashCode;
        }
        
    }
}