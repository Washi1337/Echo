using System;
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
            Bits = new BitArray(byteCount*8);
            Mask = new BitArray(byteCount * 8, true);
        }
        
        /// <summary>
        /// Creates a fully known new integer from a bit array.
        /// </summary>
        /// <param name="bits">The raw bits of the integer.</param>
        public IntegerNValue(BitArray bits)
        {
            Bits = bits;
            Mask = new BitArray(bits.Count, true);
        }

        /// <summary>
        /// Creates a partially known new integer from a bit array and a known bit mask.
        /// </summary>
        /// <param name="bits">The raw bits of the integer.</param>
        /// <param name="knownMask">The known bit mask.</param>
        public IntegerNValue(BitArray bits, BitArray knownMask)
        {
            if (bits.Count != knownMask.Count)
                throw new ArgumentException("Known bit mask does not have the same bit-length as the raw value.");
            
            Bits = bits;
            Mask = knownMask;
        }

        /// <inheritdoc />
        public override bool IsKnown => BitArrayComparer.Instance.Equals(Mask, new BitArray(Size * 8, true));

        /// <inheritdoc />
        public override int Size => Bits.Count / 8;
        
        /// <summary>
        /// Gets the raw bits of this integer. 
        /// </summary>
        public BitArray Bits
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the known bit mask of this integer. 
        /// </summary>
        public BitArray Mask
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public override IValue Copy() => new IntegerNValue(Bits, Mask);

        /// <inheritdoc />
        public override bool? GetBit(int index) => !Mask[index] ? (bool?) null : Bits[index];

        /// <inheritdoc />
        public override void SetBit(int index, bool? value)
        {
            Mask[index] = value.HasValue;
            Bits[index] = !value.HasValue || value.Value;
        }

        /// <inheritdoc />
        public override BitArray GetBits() => (BitArray) Bits.Clone();

        /// <inheritdoc />
        public override BitArray GetMask() => (BitArray) Mask.Clone();

        /// <inheritdoc />
        public override void SetBits(BitArray bits, BitArray mask)
        {
            if (bits.Count != Bits.Count)
                throw new ArgumentException("New bit value does not have the same bit-length as the original value.");
            if (bits.Count != mask.Count)
                throw new ArgumentException("Known bit mask does not have the same bit-length as the raw value.");
            
            Bits = bits;
            Mask = mask;
        }
    }
}