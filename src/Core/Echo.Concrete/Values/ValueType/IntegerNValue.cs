using System;
using System.Buffers;
using System.Collections;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known fixed size integer value. 
    /// </summary>
    public class IntegerNValue : IntegerValue
    {
        /// <summary>
        /// Creates a new zero integer.
        /// </summary>
        /// <param name="byteCount">The number of bytes to use for encoding this integer.</param>
        public IntegerNValue(int byteCount)
        {
            _bits = ArrayPool<byte>.Shared.Rent(byteCount);
            _mask = ArrayPool<byte>.Shared.Rent(byteCount);
            _mask.AsSpan().Fill(0xFF);

            Size = byteCount;
        }
        
        /// <summary>
        /// Creates a fully known new integer from a bit array.
        /// </summary>
        /// <param name="bits">The raw bits of the integer.</param>
        public IntegerNValue(Span<byte> bits)
        {
            _bits = ArrayPool<byte>.Shared.Rent(bits.Length);
            bits.CopyTo(_bits);

            _mask = ArrayPool<byte>.Shared.Rent(bits.Length);
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

            _bits = ArrayPool<byte>.Shared.Rent(bits.Length);
            bits.CopyTo(_bits);

            _mask = ArrayPool<byte>.Shared.Rent(knownMask.Length);
            knownMask.CopyTo(_mask);

            Size = bits.Length;
        }    
        
        /// <summary>
        /// Parses a (partially) known bit string into an integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        public IntegerNValue(string bitString)
        {
            _bits = ArrayPool<byte>.Shared.Rent(bitString.Length / 8);
            _mask = ArrayPool<byte>.Shared.Rent(bitString.Length / 8);

            Size = bitString.Length / 8;
            SetBits(bitString);
        }

        /// <summary>
        /// Returns the rented array to <see cref="ArrayPool{T}"/>
        /// </summary>
        ~IntegerNValue()
        {
            if (_bits is {})
            {
                ArrayPool<byte>.Shared.Return(_bits, true);
            }

            if (_mask is {})
            {
                ArrayPool<byte>.Shared.Return(_mask, true);
            }
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

        private readonly byte[] _bits;

        private readonly byte[] _mask;

        /// <inheritdoc />
        public override IValue Copy() => new IntegerNValue(_bits.AsSpan(0, Size), _mask.AsSpan(0, Size));

        /// <inheritdoc />
        public override bool? GetBit(int index)
        {
            var bits = new BitField(_bits.AsSpan(0, Size));
            var mask = new BitField(_mask.AsSpan(0, Size));
            
            return !mask[index] ? (bool?) null : bits[index];
        }

        /// <inheritdoc />
        public override void SetBit(int index, bool? value)
        {
            var bits = new BitField(_bits.AsSpan(0, Size));
            var mask = new BitField(_mask.AsSpan(0, Size));
            
            mask[index] = value.HasValue;
            bits[index] = !value.HasValue || value.Value;
        }

        /// <param name="buffer"></param>
        /// <inheritdoc />
        public override void GetBits(Span<byte> buffer) => _bits.AsSpan(0, Size).CopyTo(buffer);

        /// <param name="buffer"></param>
        /// <inheritdoc />
        public override void GetMask(Span<byte> buffer) => _mask.AsSpan(0, Size).CopyTo(buffer);

        /// <inheritdoc />
        public override void SetBits(Span<byte> bits, Span<byte> mask)
        {
            if (bits.Length != Size)
                throw new ArgumentException("New bit value does not have the same bit-length as the original value.");
            if (mask.Length != Size)
                throw new ArgumentException("Known bit mask does not have the same bit-length as the raw value.");
            
            bits.CopyTo(_bits);
            mask.CopyTo(_mask);
            Bits = (BitArray) bits.Clone();
            Mask = (BitArray) mask.Clone();
        }      
        
        /// <inheritdoc />
        public override void MarkFullyUnknown()
        {
            Mask.SetAll(false);
        }

    }
}