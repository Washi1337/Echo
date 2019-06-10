using System;
using System.Collections;
using Echo.Core.Values;

namespace Echo.Concrete.Values
{
    /// <summary>
    /// Represents a (partially) known concrete 16 bit integral value.
    /// </summary>
    public class Integer16 : PrimitiveNumberValue
    {
        public static implicit operator Integer16(ushort value)
        {
            return new Integer16(value);
        }

        public static implicit operator Integer16(short value)
        {
            return new Integer16(value);
        }
        
        public const ushort FullyKnownMask = 0xFFFF;
        
        private ushort _value;
        
        public Integer16(ushort value)
            : this(value, FullyKnownMask)
        {
        }

        public Integer16(short value)
            : this(value, FullyKnownMask)
        {
        }

        public Integer16(short value, ushort mask)
            : this(unchecked((ushort) value), mask)
        {
        }

        public Integer16(ushort value, ushort mask)
        {
            _value = value;
            Mask = mask;
        }

        /// <inheritdoc />
        public override bool IsKnown => Mask == FullyKnownMask;

        /// <inheritdoc />
        public override int Size => sizeof(ushort);

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

        public override BitArray GetBits()
        {
            return new BitArray(BitConverter.GetBytes(U16));
        }

        public override BitArray GetMask()
        {
            return new BitArray(BitConverter.GetBytes(Mask));
        }

        public override IValue Copy()
        {
            return new Integer16(U16);
        }
        
    }
}