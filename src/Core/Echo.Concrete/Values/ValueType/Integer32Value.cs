using System;
using System.Buffers.Binary;
using System.Collections;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known concrete 32 bit integral value.
    /// </summary>
    public class Integer32Value : IntegerValue
    {
        /// <summary>
        /// Wraps an unsigned 32 bit integer into a fully concrete and known instance of <see cref="Integer32Value"/>.
        /// </summary>
        /// <param name="value">The 32 bit integer to wrap.</param>
        /// <returns>The concrete 32 bit integer.</returns>
        public static implicit operator Integer32Value(ushort value)
        {
            return new Integer32Value(value);
        }

        /// <summary>
        /// Wraps a signed 32 bit integer into a fully concrete and known instance of <see cref="Integer32Value"/>.
        /// </summary>
        /// <param name="value">The 32 bit integer to wrap.</param>
        /// <returns>The concrete 32 bit integer.</returns>
        public static implicit operator Integer32Value(short value)
        {
            return new Integer32Value(value);
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 32 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        /// <returns>The 32 bit integer.</returns>
        public static implicit operator Integer32Value(string bitString)
        {
            return new Integer32Value(bitString);
        }
        
        /// <summary>
        /// Represents the bitmask that is used for a fully known concrete 32 bit integral value. 
        /// </summary>
        public const uint FullyKnownMask = 0xFFFFFFFF;
        
        private uint _value;
        
        /// <summary>
        /// Creates a new, fully known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        public Integer32Value(int value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, fully known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        public Integer32Value(uint value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer32Value(int value, uint mask)
            : this(unchecked((uint) value), mask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer32Value(uint value, uint mask)
        {
            _value = value;
            Mask = mask;
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 32 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        public Integer32Value(string bitString)
        {
            SetBits(bitString);
        }

        /// <inheritdoc />
        public override bool IsKnown => Mask == FullyKnownMask;

        /// <inheritdoc />
        public override int Size => sizeof(uint);

        /// <inheritdoc />
        public override bool? IsZero
        {
            get
            {
                if (IsKnown)
                    return U32 == 0;
                return base.IsZero;
            }
        }
        
        /// <summary>
        /// Gets the signed representation of this 32 bit value.
        /// </summary>
        public int I32
        {
            get => unchecked((int) U32);
            set => U32 = unchecked((uint) value);
        }

        /// <summary>
        /// Gets the unsigned representation of this 32 bit value.
        /// </summary>
        public uint U32
        {
            get => _value & Mask;
            set => _value = value;
        }

        /// <summary>
        /// Gets a value indicating which bits in the integer are known.
        /// If bit at location <c>i</c> equals 1, bit <c>i</c> in <see cref="I32"/> and <see cref="U32"/> is known,
        /// and unknown otherwise.  
        /// </summary>
        public uint Mask
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override bool? GetBit(int index)
        {
            if (index < 0 || index >= 32)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ((Mask >> index) & 1) == 1 ? ((U32 >> index) & 1) == 1 : (bool?) null;
        }

        /// <inheritdoc />
        public override void SetBit(int index, bool? value)
        {
            if (index < 0 || index >= 32)
                throw new ArgumentOutOfRangeException(nameof(index));

            uint mask = 1u << index;
            
            if (value.HasValue)
            {
                Mask |= mask;
                U32 = (U32 & ~mask) | ((value.Value ? 1u : 0u) << index);
            }
            else
            {
                Mask &= ~mask;
            }
        }

        /// <param name="buffer"></param>
        /// <inheritdoc />
        public override void GetBits(Span<byte> buffer) => BinaryPrimitives.WriteUInt32LittleEndian(buffer, U32);

        /// <param name="buffer"></param>
        /// <inheritdoc />
        public override void GetMask(Span<byte> buffer) => BinaryPrimitives.WriteUInt32LittleEndian(buffer, Mask);

        /// <inheritdoc />
        public override void SetBits(Span<byte> bits, Span<byte> mask)
        {
            if (bits.Length != 32 || mask.Length != 32)
                throw new ArgumentException("Number of bits is not 32.");

            U32 = BinaryPrimitives.ReadUInt32LittleEndian(bits);
            Mask = BinaryPrimitives.ReadUInt32LittleEndian(mask);
        }
        
        /// <inheritdoc />
        public override IValue Copy() => new Integer32Value(U32, Mask);

        /// <inheritdoc />
        public override void Not()
        {
            U32 = ~U32;
        }

        /// <inheritdoc />
        public override void And(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 = U32 & int32.U32;
            else
                base.And(other);
        }

        /// <inheritdoc />
        public override void Or(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 = U32 | int32.U32;
            else
                base.Or(other);
        }

        /// <inheritdoc />
        public override void Xor(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 = U32 ^ int32.U32;
            else
                base.Xor(other);
        }

        /// <inheritdoc />
        public override void Add(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 += int32.U32;
            else
                base.Add(other);
        }

        /// <inheritdoc />
        public override void Subtract(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 -= int32.U32;
            else
                base.Subtract(other);
        }

        /// <inheritdoc />
        public override void Multiply(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 *= int32.U32;
            else
                base.Multiply(other);
        }

        /// <inheritdoc />
        public override bool? IsEqualTo(IntegerValue other)
        {
            return IsKnown && other.IsKnown && other is Integer32Value int32 
                ? U32 == int32.U32 
                : (bool?) null;
        }

        /// <inheritdoc />
        public override bool? IsGreaterThan(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                return U32 > int32.U32;

            return base.IsGreaterThan(other);
        }

        /// <inheritdoc />
        public override bool? IsLessThan(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                return U32 < int32.U32;
            
            return base.IsLessThan(other);
        }
    }
}