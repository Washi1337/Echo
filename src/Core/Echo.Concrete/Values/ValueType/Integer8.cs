using System.Collections;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known concrete 8 bit integral value.
    /// </summary>
    public class Integer8 : PrimitiveNumberValue
    {
        public static implicit operator Integer8(byte value)
        {
            return new Integer8(value);
        }

        public static implicit operator Integer8(sbyte value)
        {
            return new Integer8(value);
        }
        
        public const byte FullyKnownMask = 0xFF;
        
        private byte _value;
        
        public Integer8(byte value)
            : this(value, FullyKnownMask)
        {
        }

        public Integer8(sbyte value)
            : this(value, FullyKnownMask)
        {
        }

        public Integer8(sbyte value, byte mask)
            : this(unchecked((byte) value), mask)
        {
        }

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

        public override BitArray GetBits()
        {
            return new BitArray(new[] {U8});
        }

        public override BitArray GetMask()
        {
            return new BitArray(new[] {Mask});
        }

        public override IValue Copy()
        {
            return new Integer8(U8);
        }
    }
}