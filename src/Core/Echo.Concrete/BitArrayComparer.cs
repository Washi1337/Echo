using System.Collections;
using System.Collections.Generic;

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
            
            var xRawBits = new int[x.Count / sizeof(int)];
            var yRawBits = new int[y.Count / sizeof(int)];
            x.CopyTo(xRawBits, 0);
            y.CopyTo(yRawBits, 0);

            for (int i = 0; i < xRawBits.Length; i++)
            {
                if (xRawBits[i] != yRawBits[i])
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override int GetHashCode(BitArray obj)
        {
            if (obj is null)
                return 0;

            var raw = new int[obj.Count / sizeof(int)];
            obj.CopyTo(raw, 0);
            
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