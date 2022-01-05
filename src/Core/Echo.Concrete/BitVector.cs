using System;

namespace Echo.Concrete
{
    /// <summary>
    /// Represents an array of bits for which the concrete may be known or unknown, and can be reinterpreted as
    /// different value types, and operated on using the different semantics of these types.
    /// </summary>
    public class BitVector
    {
        /// <summary>
        /// Creates a new bit vector of the provided size.
        /// </summary>
        /// <param name="count">The number of bits in the vector.</param>
        /// <param name="initialize">Indicates the bitvector should be initialized with zeroes.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when <paramref name="count"/> is not a multiple of 8.
        /// </exception>
        public BitVector(int count, bool initialize)
        {
            if (count % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(count), "The number of bits in the vector should be a multiple of 8.");

            Bits = new byte[count / 8];
            KnownMask = new byte[Bits.Length];

            if (initialize)
                KnownMask.AsSpan().Fill(0xFF);
        }
        
        /// <summary>
        /// Wraps a byte array into a fully known bit vector.
        /// </summary>
        /// <param name="bits">The raw bits to wrap.</param>
        public BitVector(byte[] bits)
        {
            Bits = bits;
            KnownMask = new byte[bits.Length];
            KnownMask.AsSpan().Fill(0xFF);
        }
        
        /// <summary>
        /// Wraps a pair of byte arrays into a partially known bit vector.
        /// </summary>
        /// <param name="bits">The raw bits to wrap.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(byte[] bits, byte[] knownMask)
        {
            Bits = bits;
            KnownMask = knownMask;
        }

        /// <summary>
        /// Gets the raw bits stored in this bit vector.
        /// </summary>
        public byte[] Bits
        {
            get;
        }

        /// <summary>
        /// Gets a bit mask indicating which bits in <see cref="Bits"/> are known.
        /// </summary>
        public byte[] KnownMask
        {
            get;
        }
        
        /// <summary>
        /// Gets the number of bits stored in the bit vector.
        /// </summary>
        public int Count => Bits.Length * 8;

        /// <summary>
        /// Creates a new span of the entire bit vector.
        /// </summary>
        /// <returns>The constructed span.</returns>
        public BitVectorSpan AsSpan() => new(Bits, KnownMask);
        
        /// <summary>
        /// Creates a new span of a portion of the bit vector that starts at a provided bit index. 
        /// </summary>
        /// <param name="bitIndex">The index to start the span at.</param>
        /// <returns>The constructed span.</returns>
        public BitVectorSpan AsSpan(int bitIndex)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");

            int byteIndex = bitIndex / 8;
            return new BitVectorSpan(Bits.AsSpan(byteIndex), KnownMask.AsSpan(byteIndex));
        }

        /// <summary>
        /// Creates a new span of a portion of the bit vector that starts at a provided bit index and has a provided length.
        /// </summary>
        /// <param name="bitIndex">The index to start the span at.</param>
        /// <param name="length">The number of bits in the slice.</param>
        /// <returns>The constructed span.</returns>
        public BitVectorSpan AsSpan(int bitIndex, int length)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");
            if (length % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");

            int byteIndex = bitIndex / 8;
            length /= 8;
            length = Math.Min(Bits.Length, length);
            return new BitVectorSpan(Bits.AsSpan(byteIndex, length), KnownMask.AsSpan(byteIndex, length));
        }
    }
}