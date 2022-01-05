using System;
using System.Text;
using Echo.Core;

namespace Echo.Concrete
{
    /// <summary>
    /// Represents a slice of an array of bits for which the concrete may be known or unknown, and can be
    /// reinterpreted as different value types, and operated on using the different semantics of these types.
    /// </summary>
    public readonly ref partial struct BitVectorSpan
    {
        [ThreadStatic]
        private static StringBuilder? _builder;

        /// <summary>
        /// Creates a new span around a pair of bits and a known bit mask.
        /// </summary>
        /// <param name="bits">The concrete bits stored in the bit vector.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when the length of <paramref name="bits"/> and <paramref name="knownMask"/> do not match up.
        /// </exception>
        public BitVectorSpan(Span<byte> bits, Span<byte> knownMask)
        {
            if (bits.Length != knownMask.Length)
                throw new ArgumentException("The number of bits is inconsistent.");

            Bits = bits;
            KnownMask = knownMask;
        }

        /// <summary>
        /// Gets or sets a single bit in the bit vector span.
        /// </summary>
        /// <param name="index">The index of the bit to get.</param>
        public Trilean this[int index]
        {
            get
            {
                int byteIndex = Math.DivRem(index, 8, out int bitIndex);
                
                if (((KnownMask[byteIndex] >> bitIndex) & 1) == 0)
                    return Trilean.Unknown;

                return ((Bits[byteIndex] >> bitIndex) & 1) == 1;
            }
            set
            {
                int byteIndex = Math.DivRem(index, 8, out int bitIndex);
                if (!value.IsKnown)
                {
                    KnownMask[byteIndex] = (byte) (KnownMask[byteIndex] & ~(1 << bitIndex));
                }
                else
                {
                    KnownMask[byteIndex] = (byte) (KnownMask[byteIndex] | (1 << bitIndex));
                    Bits[byteIndex] = (byte) (Bits[byteIndex] | ((value ? 1 : 0) << bitIndex));
                }
            }
        }

        /// <summary>
        /// Gets the raw bits stored in this bit vector span.
        /// </summary>
        public Span<byte> Bits
        {
            get;
        }

        /// <summary>
        /// Gets a bit mask indicating which bits in <see cref="Bits"/> are known.
        /// </summary>
        public Span<byte> KnownMask
        {
            get;
        }

        /// <summary>
        /// Gets the number of bits stored in the bit vector.
        /// </summary>
        public int Count => Bits.Length * 8;
        
        /// <summary>
        /// Gets a value indicating whether all bits in the vector are known. 
        /// </summary>
        public bool IsFullyKnown => KnownMask.All(0xFF);

        /// <summary>
        /// Gets a value indicating whether all bits in the vector are set to zero.
        /// </summary>
        public Trilean IsZero
        {
            get
            {
                var bits = Bits;

                if (IsFullyKnown)
                    return bits.All(0);

                var mask = KnownMask;
                for (int i = 0; i < bits.Length; i++)
                {
                    if ((bits[i] & mask[i]) != 0)
                        return Trilean.False;
                }

                return Trilean.Unknown;
            }
        }
        
        /// <summary>
        /// Forms a slice of a bit vector that starts at a provided bit index.
        /// </summary>
        /// <param name="bitIndex">The bit index to start the slice at.</param>
        /// <returns>The constructed slice.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the bit index is not a multiple of 8.</exception>
        public BitVectorSpan Slice(int bitIndex)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");

            int byteIndex = bitIndex / 8;
            return new BitVectorSpan(Bits.Slice(byteIndex), KnownMask.Slice(byteIndex));
        }

        /// <summary>
        /// Forms a slice of a bit vector that starts at a provided bit index and has a provided length.
        /// </summary>
        /// <param name="bitIndex">The bit index to start the slice at.</param>
        /// <param name="length">The number of bits in the slice.</param>
        /// <returns>The constructed slice.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the bit index is not a multiple of 8.</exception>
        public BitVectorSpan Slice(int bitIndex, int length)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");
            if (length % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");

            int byteIndex = bitIndex / 8;
            length /= 8;
            return new BitVectorSpan(Bits.Slice(byteIndex, length), KnownMask.Slice(byteIndex, length));
        }

        /// <summary>
        /// Copies all bits and known bit mask to the provided bit vector. 
        /// </summary>
        /// <param name="buffer">The bit buffer to copy the bits to.</param>
        public void CopyTo(BitVectorSpan buffer)
        {
            Bits.CopyTo(buffer.Bits);
            KnownMask.CopyTo(buffer.KnownMask);
        }

        /// <summary>
        /// Writes fully known bytes into the bit vector at the provided bit index. 
        /// </summary>
        /// <param name="bitIndex">The bit index to start writing at.</param>
        /// <param name="data">The data to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the bit index is not a multiple of 8.</exception>
        public void WriteBytes(int bitIndex, ReadOnlySpan<byte> data)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The bit index into the vector should be a multiple of 8.");
            
            data.CopyTo(Bits.Slice(bitIndex / 8));
            KnownMask.Slice(bitIndex / 8, data.Length).Fill(0xFF);
        }

        /// <summary>
        /// Writes a (partially known) bit string, where the least significant bit is at the end of the string, into
        /// the bit vector at the provided bit index.
        /// </summary>
        /// <param name="bitIndex">The bit index to start writing at.</param>
        /// <param name="binaryString">The binary string to write. This string may contain unknown bits (<c>?</c>).</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the bit index is not a multiple of 8.</exception>
        public void WriteBinaryString(int bitIndex, string binaryString)
        {
            if (binaryString.Length % 8 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(binaryString),
                    "The number of bits in the vector should be a multiple of 8.");
            }

            for (int i = 0; i < binaryString.Length; i++)
                this[bitIndex + i] = Trilean.FromChar(binaryString[binaryString.Length - i - 1]);
        }

        /// <summary>
        /// Constructs a binary string that represents the binary number stored in the bit vector.  
        /// </summary>
        /// <returns>The binary string.</returns>
        /// <remarks>
        /// When a bit is marked as unknown, its digit is replaced with a question mark (<c>?</c>). 
        /// </remarks>
        public string ToBitString()
        {
            _builder ??= new StringBuilder();
            _builder.Clear();

            for (int i = Count - 1; i >= 0; i--)
            {
                var bit = this[i];
                _builder.Append(bit.ToChar());
            }

            return _builder.ToString();
        }

        /// <summary>
        /// Constructs a string that represents the raw data stored in the bit vector as a hexadecimal byte string.  
        /// </summary>
        /// <returns>The byte string.</returns>
        /// <remarks>
        /// When any bit in a nibble is marked as unknown, its digit is replaced with a question mark (<c>?</c>). 
        /// </remarks>
        public string ToHexString()
        {
            _builder ??= new StringBuilder();
            _builder.Clear();

            // Go over each nibble.
            for (int i = 0; i < Count; i += 4)
            {
                // Get byte and bit index.
                int byteIndex = Math.DivRem(i, 8, out int bitIndex);
                bitIndex = 4 - bitIndex;
                
                char hexDigit;
                
                // Are any bits in the current nibble marked unknown?
                if (((KnownMask[byteIndex] >> bitIndex) & 0b1111) != 0b1111)
                {
                    hexDigit = '?';
                }
                else
                {
                    // Get the concrete value and transform to a hex digit.
                    int bits = ((Bits[byteIndex] >> bitIndex) & 0b1111);
                    hexDigit = bits switch
                    {
                        < 10 => (char) (bits + '0'),
                        < 16 => (char) (bits - 10 + 'A'),
                        _ => throw new ArgumentException("Invalid hex digit") // Unreachable.
                    };
                }

                _builder.Append(hexDigit);
            }

            return _builder.ToString();
        }
    }
}