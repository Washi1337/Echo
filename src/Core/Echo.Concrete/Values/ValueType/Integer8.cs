using System.Collections;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known concrete 8 bit integral value.
    /// </summary>
    public class Integer8 : PrimitiveNumberValue
    {
        /// <summary>
        /// Wraps an unsigned 8 bit integer into a fully concrete and known instance of <see cref="Integer8"/>.
        /// </summary>
        /// <param name="value">The 8 bit integer to wrap.</param>
        /// <returns>The concrete 8 bit integer.</returns>
        public static implicit operator Integer8(byte value)
        {
            return new Integer8(value);
        }

        /// <summary>
        /// Wraps a signed 8 bit integer into a fully concrete and known instance of <see cref="Integer8"/>.
        /// </summary>
        /// <param name="value">The 8 bit integer to wrap.</param>
        /// <returns>The concrete 8 bit integer.</returns>
        public static implicit operator Integer8(sbyte value)
        {
            return new Integer8(value);
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
        public Integer8(byte value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, fully known concrete 8 bit integral value.
        /// </summary>
        /// <param name="value">The raw 8 bit value.</param>
        public Integer8(sbyte value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 8 bit integral value.
        /// </summary>
        /// <param name="value">The raw 8 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer8(sbyte value, byte mask)
            : this(unchecked((byte) value), mask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 8 bit integral value.
        /// </summary>
        /// <param name="value">The raw 8 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer8(byte value, byte mask)
        {
            _value = value;
            Mask = mask;
        }

        /// <inheritdoc />
        public override bool IsKnown => Mask == FullyKnownMask;

        /// <inheritdoc />
        public override int Size => sizeof(byte);

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
        public override BitArray GetBits() => new BitArray(new[] {U8});

        /// <inheritdoc />
        public override BitArray GetMask() => new BitArray(new[] {Mask});

        /// <inheritdoc />
        public override IValue Copy() => new Integer8(U8);
    }
}