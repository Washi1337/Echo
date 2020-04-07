using System;
using System.Collections;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known concrete 64 bit integral value.
    /// </summary>
    public class Integer64 : PrimitiveNumberValue
    {
        /// <summary>
        /// Wraps an unsigned 64 bit integer into a fully concrete and known instance of <see cref="Integer64"/>.
        /// </summary>
        /// <param name="value">The 64 bit integer to wrap.</param>
        /// <returns>The concrete 64 bit integer.</returns>
        public static implicit operator Integer64(ulong value)
        {
            return new Integer64(value);
        }

        /// <summary>
        /// Wraps a signed 64 bit integer into a fully concrete and known instance of <see cref="Integer64"/>.
        /// </summary>
        /// <param name="value">The 64 bit integer to wrap.</param>
        /// <returns>The concrete 64 bit integer.</returns>
        public static implicit operator Integer64(long value)
        {
            return new Integer64(value);
        }
        
        /// <summary>
        /// Represents the bitmask that is used for a fully known concrete 64 bit integral value. 
        /// </summary>
        public const ulong FullyKnownMask = 0xFFFFFFFF_FFFFFFFF;
        
        private ulong _value;
        
        /// <summary>
        /// Creates a new, fully known concrete 64 bit integral value.
        /// </summary>
        /// <param name="value">The raw 64 bit value.</param>
        public Integer64(ulong value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, fully known concrete 64 bit integral value.
        /// </summary>
        /// <param name="value">The raw 64 bit value.</param>
        public Integer64(long value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 64 bit integral value.
        /// </summary>
        /// <param name="value">The raw 64 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer64(long value, ulong mask)
            : this(unchecked((ulong) value), mask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 64 bit integral value.
        /// </summary>
        /// <param name="value">The raw 64 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer64(ulong value, ulong mask)
        {
            _value = value;
            Mask = mask;
        }

        /// <inheritdoc />
        public override bool IsKnown => Mask == FullyKnownMask;

        /// <inheritdoc />
        public override int Size => sizeof(ulong);

        /// <summary>
        /// Gets the signed representation of this 64 bit value.
        /// </summary>
        public long I64
        {
            get => unchecked((long) U64);
            set => U64 = unchecked((ulong) value);
        }

        /// <summary>
        /// Gets the unsigned representation of this 64 bit value.
        /// </summary>
        public ulong U64
        {
            get => _value & Mask;
            set => _value = value;
        }

        /// <summary>
        /// Gets a value indicating which bits in the integer are known.
        /// If bit at location <c>i</c> equals 1, bit <c>i</c> in <see cref="I64"/> and <see cref="U64"/> is known,
        /// and unknown otherwise.  
        /// </summary>
        public ulong Mask
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override BitArray GetBits() => new BitArray(BitConverter.GetBytes(U64));

        /// <inheritdoc />
        public override BitArray GetMask() => new BitArray(BitConverter.GetBytes(Mask));

        /// <inheritdoc />
        public override IValue Copy() => new Integer64(U64);
    }
}