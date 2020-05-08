using System;
using System.Buffers.Binary;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known concrete 16 bit integral value.
    /// </summary>
    public class Integer16Value : IntegerValue
    {
        /// <summary>
        /// Wraps an unsigned 16 bit integer into a fully concrete and known instance of <see cref="Integer16Value"/>.
        /// </summary>
        /// <param name="value">The 16 bit integer to wrap.</param>
        /// <returns>The concrete 16 bit integer.</returns>
        public static implicit operator Integer16Value(ushort value)
        {
            return new Integer16Value(value);
        }

        /// <summary>
        /// Wraps a signed 16 bit integer into a fully concrete and known instance of <see cref="Integer16Value"/>.
        /// </summary>
        /// <param name="value">The 16 bit integer to wrap.</param>
        /// <returns>The concrete 16 bit integer.</returns>
        public static implicit operator Integer16Value(short value)
        {
            return new Integer16Value(value);
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 16 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        /// <returns>The 16 bit integer.</returns>
        public static implicit operator Integer16Value(string bitString)
        {
            return new Integer16Value(bitString);
        }

        /// <summary>
        /// Represents the bitmask that is used for a fully known concrete 16 bit integral value. 
        /// </summary>
        public const ushort FullyKnownMask = 0xFFFF;

        private ushort _value;

        /// <summary>
        /// Creates a new, fully known concrete 16 bit integral value.
        /// </summary>
        /// <param name="value">The raw 16 bit value.</param>
        public Integer16Value(ushort value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, fully known concrete 16 bit integral value.
        /// </summary>
        /// <param name="value">The raw 16 bit value.</param>
        public Integer16Value(short value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 16 bit integral value.
        /// </summary>
        /// <param name="value">The raw 16 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer16Value(short value, ushort mask)
            : this(unchecked((ushort) value), mask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 16 bit integral value.
        /// </summary>
        /// <param name="value">The raw 16 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer16Value(ushort value, ushort mask)
        {
            _value = value;
            Mask = mask;
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 16 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        public Integer16Value(string bitString)
        {
            SetBits(bitString);
        }

        /// <inheritdoc />
        public override bool IsKnown => Mask == FullyKnownMask;

        /// <inheritdoc />
        public override int Size => sizeof(ushort);

        /// <inheritdoc />
        public override bool? IsZero
        {
            get
            {
                if (IsKnown)
                    return U16 == 0;
                return base.IsZero;
            }
        }

        /// <summary>
        /// Gets the signed representation of this 16 bit value.
        /// </summary>
        public short I16
        {
            get => unchecked((short) U16);
            set => U16 = unchecked((ushort) value);
        }

        /// <summary>
        /// Gets the unsigned representation of this 16 bit value.
        /// </summary>
        public ushort U16
        {
            get => (ushort) (_value & Mask);
            set => _value = value;
        }

        /// <summary>
        /// Gets a value indicating which bits in the integer are known.
        /// If bit at location <c>i</c> equals 1, bit <c>i</c> in <see cref="I16"/> and <see cref="U16"/> is known,
        /// and unknown otherwise.  
        /// </summary>
        public ushort Mask
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override bool? GetBit(int index)
        {
            if (index < 0 || index >= 16)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ((Mask >> index) & 1) == 1 ? ((U16 >> index) & 1) == 1 : (bool?) null;
        }

        /// <inheritdoc />
        public override void SetBit(int index, bool? value)
        {
            if (index < 0 || index >= 16)
                throw new ArgumentOutOfRangeException(nameof(index));

            ushort mask = (ushort) (1 << index);

            if (value.HasValue)
            {
                Mask |= mask;
                U16 = (ushort) ((U16 & ~mask) | ((value.Value ? 1 : 0) << index));
            }
            else
            {
                U16 = (ushort) (U16 & ~mask);
                Mask &= (ushort) ~mask;
            }
        }

        /// <inheritdoc />
        public override void GetBits(Span<byte> buffer) => BinaryPrimitives.WriteUInt16LittleEndian(buffer, U16);

        /// <inheritdoc />
        public override void GetMask(Span<byte> buffer) => BinaryPrimitives.WriteUInt16LittleEndian(buffer, Mask);

        /// <inheritdoc />
        public override void SetBits(Span<byte> bits, Span<byte> mask)
        {
            if (bits.Length != 2 || mask.Length != 2)
                throw new ArgumentException("Number of bits is not 16.");
            
            U16 = BinaryPrimitives.ReadUInt16LittleEndian(bits);
            Mask = BinaryPrimitives.ReadUInt16LittleEndian(mask);
        }

        /// <inheritdoc />
        public override IValue Copy() => new Integer16Value(U16, Mask);

        /// <inheritdoc />
        public override void Not()
        {
            U16 = unchecked((ushort) ~U16);
        }

        /// <inheritdoc />
        public override void And(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer16Value int16)
                U16 = (ushort) (U16 & int16.U16);
            else
                base.And(other);
        }

        /// <inheritdoc />
        public override void Or(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer16Value int16)
                U16 = (ushort) (U16 | int16.U16);
            else
                base.Or(other);
        }

        /// <inheritdoc />
        public override void Xor(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer16Value int16)
                U16 = (ushort) (U16 ^ int16.U16);
            else
                base.Xor(other);
        }

        /// <inheritdoc />
        public override void Add(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer16Value int16)
                U16 += int16.U16;
            else
                base.Add(other);
        }

        /// <inheritdoc />
        public override void Subtract(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer16Value int16)
                U16 -= int16.U16;
            else
                base.Subtract(other);
        }

        /// <inheritdoc />
        public override void Multiply(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer16Value int16)
                U16 *= int16.U16;
            else
                base.Multiply(other);
        }

        /// <inheritdoc />
        public override bool? IsEqualTo(IntegerValue other)
        {
            if (other is Integer16Value int16)
            {
                if (IsKnown && other.IsKnown)
                    return U16 == int16.U16;
                return U16 == int16.U16 ? null : (bool?) false;
            }

            return base.IsEqualTo(other);
        }

        /// <inheritdoc />
        public override bool? IsGreaterThan(IntegerValue other, bool signed)
        {
            if (IsKnown && other.IsKnown && other is Integer16Value int16)
                return signed ? I16 > int16.I16 : U16 > int16.U16;

            return base.IsGreaterThan(other, signed);
        }

        /// <inheritdoc />
        public override bool? IsLessThan(IntegerValue other, bool signed)
        {
            if (IsKnown && other.IsKnown && other is Integer16Value int16)
                return signed ? I16 < int16.I16 : U16 < int16.U16;

            return base.IsLessThan(other, signed);
        }

        /// <inheritdoc />
        public override void MarkFullyUnknown()
        {
            Mask = 0;
        }
    }
}