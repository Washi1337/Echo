using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Echo.Core;

namespace Echo.Concrete
{
    /// <summary>
    /// Represents a slice of an array of bits for which the concrete may be known or unknown, and can be
    /// reinterpreted as different value types, and operated on using the different semantics of these types.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public readonly ref partial struct BitVectorSpan
    {
        [ThreadStatic]
        private static StringBuilder? _builder;
        
        [ThreadStatic]
        private static List<BitVector?>? _temporaryVectors; 

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
                    Bits[byteIndex] = (byte) ((Bits[byteIndex] & ~(1 << bitIndex)) | ((value ? 1 : 0) << bitIndex));
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
        /// Gets the number of bytes stored in the bit vector.
        /// </summary>
        public int ByteCount => Bits.Length;
        
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal string DebuggerDisplay
        {
            get
            {
                string? suffix = Count switch
                {
                    >= 64 => Count < 800 ? $"bytes: {ToHexString()}" : null,
                    > 0 => $"bits: 0b{ToBitString()}",
                    _ => null
                };

                return suffix is not null 
                    ? $"Count = {Count} ({suffix})"
                    : $"Count = {Count}";
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
        /// Clears the bit vector with zeroes.
        /// </summary>
        public void Clear()
        {
            Bits.Fill(0);
            MarkFullyKnown();
        }

        /// <summary>
        /// Marks the entire bit vector fully known, treating all bits in <see cref="Bits"/> as actual data.
        /// </summary>
        /// <remarks>
        /// This is effectively setting all bits in <see cref="KnownMask"/>.
        /// </remarks>
        public void MarkFullyKnown() => KnownMask.Fill(0xFF);
        
        /// <summary>
        /// Marks the entire bit vector fully unknown.
        /// </summary>
        /// <remarks>
        /// This is effectively clearing all bits in <see cref="KnownMask"/>. It does not change the value of <see cref="Bits"/>.
        /// </remarks>
        public void MarkFullyUnknown() => KnownMask.Fill(0);

        /// <summary>
        /// Reads a native integer from the vector.
        /// </summary>
        /// <param name="bitIndex">The bit index to start reading at.</param>
        /// <param name="is32Bit">A value indicating whether the native integer is 32 or 64 bits wide.</param>
        /// <returns>The read integer.</returns>
        public long ReadNativeInteger(int bitIndex, bool is32Bit) => is32Bit ? U32 : I64;

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
        /// Writes a fully known native integer into the bit vector at the provided bit index.
        /// </summary>
        /// <param name="bitIndex">The bit index to start writing at.</param>
        /// <param name="value">The native integer to write.</param>
        /// <param name="is32Bit">A value indicating whether the native integer is 32 or 64 bits wide.</param>
        public void WriteNativeInteger(int bitIndex, long value, bool is32Bit)
        {
            if (is32Bit)
            {
                Span<int> x = stackalloc int[1];
                x[0] = (int) value;
                WriteBytes(bitIndex, MemoryMarshal.Cast<int, byte>(x));
            }
            else
            {
                Span<long> x = stackalloc long[1];
                x[0] = (long) value;
                WriteBytes(bitIndex, MemoryMarshal.Cast<long, byte>(x));
            }
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
            _builder.EnsureCapacity(Count);

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
            _builder.EnsureCapacity(Count / 4);

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
        
        private void AssertSameBitSize(BitVectorSpan other)
        {
            if (Count != other.Count)
                throw new ArgumentException($"Cannot perform a binary operation on a {other.Count} bit vector and a {Count} bit vector.");
        }

        /// <summary>
        /// Determines whether the current bit vector is equal to another bit vector.
        /// </summary>
        /// <param name="other">The other bit vector.</param>
        /// <returns>
        /// <see cref="Trilean.True"/> if the bit vector are equal, <see cref="Trilean.False"/> if not, and
        /// <see cref="Trilean.Unknown"/> if the conclusion of the comparison is not certain.
        /// </returns>
        public Trilean IsEqualTo(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (IsFullyKnown && other.IsFullyKnown)
                return Equals(other);

            // Check if we definitely know this is not equal to the other.
            for (int i = 0; i < Bits.Length; i++)
            {
                (byte bitsA, byte knownA) = (Bits[i], KnownMask[i]);
                (byte bitsB, byte knownB) = (other.Bits[i], other.KnownMask[i]);

                if ((bitsA & knownA & knownB) != (bitsB & knownA & knownB))
                    return Trilean.False;
            }

            return Trilean.Unknown;
        }

        /// <summary>
        /// Compares two <see cref="BitVectorSpan"/>'s
        /// </summary>
        /// <remarks>
        /// This overload exists to avoid boxing allocations.
        /// </remarks>
        /// <param name="other">The <see cref="BitVectorSpan"/> to compare to</param>
        /// <returns>Whether the two <see cref="BitVectorSpan"/>'s are equal</returns>
        public bool Equals(BitVectorSpan other)
        {
            return Bits.SequenceEqual(other.Bits) && KnownMask.SequenceEqual(other.KnownMask);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            // Since this is a ref struct, it will
            // never equal any reference type
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode() => Bits.GetSequenceHashCode() ^ KnownMask.GetSequenceHashCode();

        private static BitVectorSpan GetTemporaryBitVector(int index, int count)
        {
            _temporaryVectors ??= new List<BitVector?>();
            while (_temporaryVectors.Count <= index)
                _temporaryVectors.Add(null);

            var vector = _temporaryVectors[index];
            if (vector is null || vector.Count < count)
            {
                vector = new BitVector(count, false);
                _temporaryVectors[index] = vector;
            }

            return vector.AsSpan(0, count);
        }
    }
}