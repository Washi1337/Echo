using System;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known concrete 8 bit integral value.
    /// </summary>
    public class Integer8Value : IntegerValue
    {
        /// <summary>
        /// Wraps an unsigned 8 bit integer into a fully concrete and known instance of <see cref="Integer8Value"/>.
        /// </summary>
        /// <param name="value">The 8 bit integer to wrap.</param>
        /// <returns>The concrete 8 bit integer.</returns>
        public static implicit operator Integer8Value(byte value)
        {
            return new Integer8Value(value);
        }

        /// <summary>
        /// Wraps a signed 8 bit integer into a fully concrete and known instance of <see cref="Integer8Value"/>.
        /// </summary>
        /// <param name="value">The 8 bit integer to wrap.</param>
        /// <returns>The concrete 8 bit integer.</returns>
        public static implicit operator Integer8Value(sbyte value)
        {
            return new Integer8Value(value);
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 8 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        /// <returns>The 8 bit integer.</returns>
        public static implicit operator Integer8Value(string bitString)
        {
            return new Integer8Value(bitString);
        }

        /// <summary>
        /// Represents the bitmask that is used for a fully known concrete 8 bit integral value. 
        /// </summary>
        public const byte FullyKnownMask = 0xFF;

        private byte _value;

        /// <summary>
        /// Creates a new, fully known concrete 8 bit integral value.
        /// </summary>
        /// <param name="value">The raw 8 bit value.</param>
        public Integer8Value(byte value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, fully known concrete 8 bit integral value.
        /// </summary>
        /// <param name="value">The raw 8 bit value.</param>
        public Integer8Value(sbyte value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 8 bit integral value.
        /// </summary>
        /// <param name="value">The raw 8 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer8Value(sbyte value, byte mask)
            : this(unchecked((byte) value), mask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 8 bit integral value.
        /// </summary>
        /// <param name="value">The raw 8 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer8Value(byte value, byte mask)
        {
            _value = value;
            Mask = mask;
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 8 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        public Integer8Value(string bitString)
        {
            SetBits(bitString);
        }

        /// <inheritdoc />
        public override bool IsKnown => Mask == FullyKnownMask;

        /// <inheritdoc />
        public override int Size => sizeof(byte);

        /// <inheritdoc />
        public override bool? IsZero
        {
            get
            {
                if (IsKnown)
                    return U8 == 0;
                return base.IsZero;
            }
        }

        /// <summary>
        /// Gets the signed representation of this 8 bit value.
        /// </summary>
        public sbyte I8
        {
            get => unchecked((sbyte) U8);
            set => U8 = unchecked((byte) value);
        }

        /// <summary>
        /// Gets the unsigned representation of this 8 bit value.
        /// </summary>
        public byte U8
        {
            get => (byte) (_value & Mask);
            set => _value = value;
        }

        /// <summary>
        /// Gets a value indicating which bits in the integer are known.
        /// If bit at location <c>i</c> equals 1, bit <c>i</c> in <see cref="I8"/> and <see cref="U8"/> is known,
        /// and unknown otherwise.  
        /// </summary>
        public byte Mask
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override bool? GetBit(int index)
        {
            if (index < 0 || index >= 8)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ((Mask >> index) & 1) == 1 ? ((U8 >> index) & 1) == 1 : (bool?) null;
        }

        /// <inheritdoc />
        public override void SetBit(int index, bool? value)
        {
            if (index < 0 || index >= 8)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte mask = (byte) (1 << index);

            if (value.HasValue)
            {
                Mask |= mask;
                U8 = (byte) ((U8 & ~mask) | ((value.Value ? 1 : 0) << index));
            }
            else
            {
                Mask &= (byte) ~mask;
            }
        }

        /// <param name="buffer"></param>
        /// <inheritdoc />
        public override void GetBits(Span<byte> buffer) => buffer[0] = U8;

        /// <param name="buffer"></param>
        /// <inheritdoc />
        public override void GetMask(Span<byte> buffer) => buffer[0] = Mask;

        /// <inheritdoc />
        public override void SetBits(Span<byte> bits, Span<byte> mask)
        {
            if (bits.Length != 1 || mask.Length != 1)
                throw new ArgumentException("Number of bits is not 8.");
            
            U8 = bits[0];
            Mask = mask[0];
        }

        /// <inheritdoc />
        public override IValue Copy() => new Integer8Value(U8, Mask);

        /// <inheritdoc />
        public override void Not()
        {
            U8 = unchecked((byte) ~U8);
        }

        /// <inheritdoc />
        public override void And(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer8Value int8)
                U8 = (byte) (U8 & int8.U8);
            else
                base.And(other);
        }

        /// <inheritdoc />
        public override void Or(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer8Value int8)
                U8 = (byte) (U8 | int8.U8);
            else
                base.Or(other);
        }

        /// <inheritdoc />
        public override void Xor(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer8Value int8)
                U8 = (byte) (U8 ^ int8.U8);
            else
                base.Xor(other);
        }

        /// <inheritdoc />
        public override void Add(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer8Value int8)
                U8 += int8.U8;
            else
                base.Add(other);
        }

        /// <inheritdoc />
        public override void Subtract(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer8Value int8)
                U8 -= int8.U8;
            else
                base.Subtract(other);
        }

        /// <inheritdoc />
        public override void Multiply(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer8Value int8)
                U8 *= int8.U8;
            else
                base.Multiply(other);
        }

        /// <inheritdoc />
        public override bool? IsEqualTo(IntegerValue other)
        {
            return IsKnown && other.IsKnown && other is Integer8Value int8
                ? U8 == int8.U8
                : (bool?) null;
        }

        /// <inheritdoc />
        public override bool? IsGreaterThan(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer8Value int8)
                return U8 > int8.U8;

            return base.IsGreaterThan(other);
        }

        /// <inheritdoc />
        public override bool? IsLessThan(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer8Value int8)
                return U8 < int8.U8;

            return base.IsLessThan(other);
        }
        
        /// <inheritdoc />
        public override void MarkFullyUnknown()
        {
            Mask = 0;
        }
    }
}