using System;
using System.Buffers;
using System.Collections;
using Echo.Core;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known fixed size integer value. 
    /// </summary>
    public class IntegerNValue : IntegerValue
    {
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Create();
        
        private readonly byte[] _bits;
        private readonly byte[] _mask;

        /// <summary>
        /// Creates a new zero integer.
        /// </summary>
        /// <param name="byteCount">The number of bytes to use for encoding this integer.</param>
        public IntegerNValue(int byteCount)
        {
            _bits = Pool.Rent(byteCount);
            _bits.AsSpan().Fill(0);
            _mask = Pool.Rent(byteCount);
            _mask.AsSpan().Fill(0xFF);

            Size = byteCount;
        }
        
        /// <summary>
        /// Creates a fully known new integer from a bit array.
        /// </summary>
        /// <param name="bits">The raw bits of the integer.</param>
        public IntegerNValue(Span<byte> bits)
        {
            _bits = Pool.Rent(bits.Length);
            bits.CopyTo(_bits);

            _mask = Pool.Rent(bits.Length);
            _mask.AsSpan().Fill(0xFF);

            Size = bits.Length;
        }

        /// <summary>
        /// Creates a partially known new integer from a bit array and a known bit mask.
        /// </summary>
        /// <param name="bits">The raw bits of the integer.</param>
        /// <param name="knownMask">The known bit mask.</param>
        public IntegerNValue(Span<byte> bits, Span<byte> knownMask)
        {
            if (bits.Length != knownMask.Length)
                throw new ArgumentException("Known bit mask does not have the same bit-length as the raw value.");

            _bits = Pool.Rent(bits.Length);
            bits.CopyTo(_bits);

            _mask = Pool.Rent(knownMask.Length);
            knownMask.CopyTo(_mask);

            Size = bits.Length;
        }    
        
        /// <summary>
        /// Parses a (partially) known bit string into an integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        public IntegerNValue(string bitString)
        {
            _bits = Pool.Rent(bitString.Length / 8);
            _mask = Pool.Rent(bitString.Length / 8);

            Size = bitString.Length / 8;
            SetBits(bitString);
        }

        /// <summary>
        /// Returns the rented array to <see cref="ArrayPool{T}"/>
        /// </summary>
        ~IntegerNValue()
        {
            if (_bits is {})
                Pool.Return(_bits, true);

            if (_mask is {})
                Pool.Return(_mask, true);
        }

        /// <inheritdoc />
        public override bool IsKnown
        {
            get
            {
                for (var i = 0; i < Size; i++)
                {
                    if (_mask[i] != 0xFF)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <inheritdoc />
        public override int Size
        {
            get;
        }

        private Span<byte> Bits
        {
            get => _bits.AsSpan(0, Size);
        }

        private Span<byte> Mask
        {
            get => _mask.AsSpan(0, Size);
        }

        /// <inheritdoc />
        public override IValue Copy() => new IntegerNValue(Bits, Mask);

        /// <inheritdoc />
        public override Trilean GetBit(int index)
        {
            var bits = new BitField(Bits);
            var mask = new BitField(Mask);
            
            return mask[index]
                ? bits[index] 
                : Trilean.Unknown;
        }

        /// <inheritdoc />
        public override void SetBit(int index, Trilean value)
        {
            var bits = new BitField(Bits);
            var mask = new BitField(Mask);
            
            mask[index] = value.IsKnown;
            bits[index] = value.ToBooleanOrFalse();
        }

        /// <inheritdoc />
        public override void GetBits(Span<byte> buffer) => Bits.CopyTo(buffer);

        /// <inheritdoc />
        public override void GetMask(Span<byte> buffer) => Mask.CopyTo(buffer);

        /// <inheritdoc />
        public override void SetBits(ReadOnlySpan<byte> bits, ReadOnlySpan<byte> mask)
        {
            if (bits.Length != Size)
                throw new ArgumentException("New bit value does not have the same bit-length as the original value.");
            if (mask.Length != Size)
                throw new ArgumentException("Known bit mask does not have the same bit-length as the raw value.");
            
            bits.CopyTo(_bits);
            mask.CopyTo(_mask);
        }      
        
        /// <inheritdoc />
        public override void MarkFullyUnknown()
        {
            Mask.Fill(0);
        }

    }
}