using System;
using System.ComponentModel;
using System.Diagnostics;
using Echo.Core;

namespace Echo.Concrete
{
    /// <summary>
    /// Represents an array of bits for which the concrete may be known or unknown, and can be reinterpreted as
    /// different value types, and operated on using the different semantics of these types.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class BitVector : ICloneable
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
        /// Creates a new fully known 8-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(sbyte value)
        {
            Bits = new byte[] {unchecked((byte) value)};
            KnownMask = new byte[] {0xFF};
        }

        /// <summary>
        /// Creates a new partially known 8-wide bit vector. 
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(sbyte bits, byte knownMask)
        {
            Bits = BitConverter.GetBytes(bits);
            KnownMask = BitConverter.GetBytes(knownMask);
        }
        
        /// <summary>
        /// Creates a new fully known 8-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(byte value)
        {
            Bits = new[] {value};
            KnownMask = new byte[] {0xFF};
        }

        /// <summary>
        /// Creates a new partially known 8-wide bit vector. 
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(byte bits, byte knownMask)
        {
            Bits = BitConverter.GetBytes(bits);
            KnownMask = BitConverter.GetBytes(knownMask);
        }
        
        /// <summary>
        /// Creates a new fully known 16-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(short value)
        {
            Bits = BitConverter.GetBytes(value);
            KnownMask = new byte[] {0xFF, 0xFF};
        }

        /// <summary>
        /// Creates a new partially known 16-wide bit vector. 
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(short bits, ushort knownMask)
        {
            Bits = BitConverter.GetBytes(bits);
            KnownMask = BitConverter.GetBytes(knownMask);
        }

        /// <summary>
        /// Creates a new fully known 16-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(ushort value)
        {
            Bits = BitConverter.GetBytes(value);
            KnownMask = new byte[] {0xFF, 0xFF};
        }

        /// <summary>
        /// Creates a new partially known 16-wide bit vector. 
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(ushort bits, ushort knownMask)
        {
            Bits = BitConverter.GetBytes(bits);
            KnownMask = BitConverter.GetBytes(knownMask);
        }
        
        /// <summary>
        /// Creates a new fully known 32-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(int value)
        {
            Bits = BitConverter.GetBytes(value);
            KnownMask = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
        }
        
        /// <summary>
        /// Creates a new partially known 32-wide bit vector. 
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(int bits, uint knownMask)
        {
            Bits = BitConverter.GetBytes(bits);
            KnownMask = BitConverter.GetBytes(knownMask);
        }
        
        /// <summary>
        /// Creates a new fully known 32-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(uint value)
        {
            Bits = BitConverter.GetBytes(value);
            KnownMask = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
        }
        
        /// <summary>
        /// Creates a new partially known 32-wide bit vector. 
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(uint bits, uint knownMask)
        {
            Bits = BitConverter.GetBytes(bits);
            KnownMask = BitConverter.GetBytes(knownMask);
        }

        /// <summary>
        /// Creates a new fully known 64-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(long value)
        {
            Bits = BitConverter.GetBytes(value);
            KnownMask = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
        }
        
        /// <summary>
        /// Creates a new partially known 64-wide bit vector. 
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(long bits, ulong knownMask)
        {
            Bits = BitConverter.GetBytes(bits);
            KnownMask = BitConverter.GetBytes(knownMask);
        }
        
        /// <summary>
        /// Creates a new fully known 64-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(ulong value)
        {
            Bits = BitConverter.GetBytes(value);
            KnownMask = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
        }
        
        /// <summary>
        /// Creates a new partially known 64-wide bit vector. 
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="knownMask">The bitmask indicating which bits in <paramref name="bits"/> are known.</param>
        public BitVector(ulong bits, ulong knownMask)
        {
            Bits = BitConverter.GetBytes(bits);
            KnownMask = BitConverter.GetBytes(knownMask);
        }

        /// <summary>
        /// Creates a new fully known 32-wide bit vector based on a floating point number. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(float value)
        {
            Bits = BitConverter.GetBytes(value);
            KnownMask = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
        }

        /// <summary>
        /// Creates a new fully known 64-wide bit vector based on a floating point number. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public BitVector(double value)
        {
            Bits = BitConverter.GetBytes(value);
            KnownMask = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal string DebuggerDisplay => AsSpan().DebuggerDisplay;
        
        /// <summary>
        /// Gets the number of bits stored in the bit vector.
        /// </summary>
        public int Count => Bits.Length * 8;

        /// <summary>
        /// Gets the number of bytes stored in the bit vector.
        /// </summary>
        public int ByteCount => Bits.Length;

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
        
        /// <summary>
        /// Parses a binary string, where the least significant bit is at the end of the string, into a bit vector.
        /// </summary>
        /// <param name="binaryString">The binary string to parse. This string may contain unknown bits (<c>?</c>).</param>
        /// <returns>The parsed bit vector.</returns>
        public static BitVector ParseBinary(string binaryString)
        {
            if (binaryString.Length % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(binaryString), "The number of bits in the vector should be a multiple of 8.");

            var result = new BitVector(binaryString.Length, false);
            result.AsSpan().WriteBinaryString(binaryString);
            return result;
        }

        /// <summary>
        /// Deep copies the bit vector.
        /// </summary>
        /// <returns>The copied bit vector.</returns>
        public BitVector Clone()
        {
            byte[] bits = new byte[Bits.Length];
            Buffer.BlockCopy(Bits, 0, bits, 0, bits.Length);
            byte[] mask = new byte[KnownMask.Length];
            Buffer.BlockCopy(KnownMask, 0, mask, 0, mask.Length);
            return new BitVector(bits, mask);
        }

        /// <summary>
        /// Deep copies the bit vector.
        /// </summary>
        /// <param name="pool">The pool to rent the cloned bitvector from.</param>
        /// <returns>The copied bit vector.</returns>
        public BitVector Clone(BitVectorPool pool)
        {
            var result = pool.Rent(Count, false);
            
            Buffer.BlockCopy(Bits, 0, result.Bits, 0, Bits.Length);
            Buffer.BlockCopy(KnownMask, 0, result.KnownMask, 0, KnownMask.Length);

            return result;
        }

        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Allocates a new bit vector that contains the same data, but is extended or truncated to a new size.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        /// <param name="signExtend">When <paramref name="newSize"/> is larger than the original size, a value
        /// indicating whether the vector should be sign extended or not.</param>
        /// <returns>The new vector.</returns>
        public BitVector Resize(int newSize, bool signExtend)
        {
            var result = new BitVector(newSize, false);
            CopyDataAndSignExtend(result, signExtend);
            return result;   
        }

        /// <summary>
        /// Rents a bit vector from a pool that contains the same data, but is extended or truncated to a new size.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        /// <param name="signExtend">When <paramref name="newSize"/> is larger than the original size, a value
        /// indicating whether the vector should be sign extended or not.</param>
        /// <param name="pool">The pool to rent the new vector from.</param>
        /// <returns>The new vector.</returns>
        public BitVector Resize(int newSize, bool signExtend, BitVectorPool pool)
        {
            var result = pool.Rent(newSize, false);
            CopyDataAndSignExtend(result, signExtend);
            return result;
        }

        private void CopyDataAndSignExtend(BitVector result, bool signExtend)
        {
            // Copy over the original data from the current bitvector into the new one.
            AsSpan(0, Math.Min(result.Count, Count)).CopyTo(result.AsSpan());

            // Sign extend the data if required.
            if (result.Count > Count)
            {
                var bit = AsSpan().GetMsb();
                var remainder = result.AsSpan(Count);

                if (!signExtend)
                {
                    remainder.Bits.Fill(0x00);
                    remainder.KnownMask.Fill(0xFF);
                }
                else
                {
                    switch (bit.Value)
                    {
                        case TrileanValue.False:
                            remainder.Bits.Fill(0x00);
                            remainder.KnownMask.Fill(0xFF);
                            break;

                        case TrileanValue.True:
                            remainder.Bits.Fill(0xFF);
                            remainder.KnownMask.Fill(0xFF);
                            break;

                        case TrileanValue.Unknown:
                            remainder.KnownMask.Fill(0x00);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        /// <inheritdoc />
        public override string ToString() => AsSpan().ToBitString();

        /// <summary>
        /// Creates a new fully known 8-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(sbyte value) => new(value);
        
        /// <summary>
        /// Creates a new fully known 8-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(byte value) => new(value);
        
        /// <summary>
        /// Creates a new fully known 16-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(short value) => new(value);
        
        /// <summary>
        /// Creates a new fully known 16-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(ushort value) => new(value);
        
        /// <summary>
        /// Creates a new fully known 32-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(int value) => new(value);
        
        /// <summary>
        /// Creates a new fully known 32-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(uint value) => new(value);

        /// <summary>
        /// Creates a new fully known 64-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(long value) => new(value);
        
        /// <summary>
        /// Creates a new fully known 64-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(ulong value) => new(value);

        /// <summary>
        /// Creates a new fully known 32-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(float value) => new(value);
        
        /// <summary>
        /// Creates a new fully known 64-wide bit vector. 
        /// </summary>
        /// <param name="value">The bits.</param>
        public static implicit operator BitVector(double value) => new(value);
    }
}