using System;
using System.Collections;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a primitive integral value that might contain unknown bits.
    /// </summary>
    public abstract class IntegerValue : IConcreteValue
    {
        /// <inheritdoc />
        public abstract bool IsKnown
        {
            get;
        }

        /// <inheritdoc />
        public abstract int Size
        {
            get;
        }

        /// <inheritdoc />
        public bool IsValueType => true;

        /// <summary>
        /// Reads a single bit value at the provided index.
        /// </summary>
        /// <param name="index">The index of the bit to read.</param>
        /// <returns>The read bit, or <c>null</c> if the bit value is unknown.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when an invalid index was provided.</exception>
        public abstract bool? GetBit(int index);

        /// <summary>
        /// Writes a single bit value at the provided index.
        /// </summary>
        /// <param name="index">The index of the bit to write.</param>
        /// <param name="value">The new value of the bit to write. <c>null</c> indicates an unknown bit value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when an invalid index was provided.</exception>
        public abstract void SetBit(int index, bool? value);
        
        /// <summary>
        /// Gets the raw bits of the primitive value.
        /// </summary>
        /// <returns>The raw bits.</returns>
        /// <remarks>
        /// The bits returned by this method assume the value is known entirely. Any bit that is marked unknown will be
        /// set to 0. 
        /// </remarks>
        public abstract BitArray GetBits();

        /// <summary>
        /// Gets the bit mask indicating the bits that are known.  
        /// </summary>
        /// <returns>
        /// The bit mask. If bit at location <c>i</c> equals 1, bit <c>i</c> is known, and unknown otherwise.
        /// </returns>
        public abstract BitArray GetMask();

        /// <summary>
        /// Replaces the raw contents of the integer with the provided bits and known mask.
        /// </summary>
        /// <param name="bits">The new bit values.</param>
        /// <param name="mask">The new bit mask indicating the known bits.</param>
        public abstract void SetBits(BitArray bits, BitArray mask);

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new char[Size * 8];
            
            for (int i = 0; i < result.Length; i++)
            {
                bool? bit = GetBit(i);
                result[result.Length - i - 1] = bit.HasValue
                    ? bit.Value ? '1' : '0'
                    : '?';
            }
            
            return new string(result);
        }

        /// <inheritdoc />
        public abstract IValue Copy();

        /// <summary>
        /// Adds a second (partially) known integer to the current integer. 
        /// </summary>
        /// <param name="other">The integer to add.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match.</exception>
        public virtual void Add(IntegerValue other)
        {
            if (Size != other.Size)
                throw new ArgumentException($"Cannot add a {other.Size * 8} bit integer to a {Size * 8} bit integer.");

            var sum = new BitArray(Size * 8);
            var mask = new BitArray(Size * 8, true);

            bool? carry = false;
            for (int i = 0; i < sum.Length; i++)
            {
                bool? a = GetBit(i);
                bool? b = other.GetBit(i);

                (bool? s, bool? c) = (a, b, carry) switch
                {
                    (false, false, false) => ((bool?) false, (bool?) false),
                    (false, false, true) => (true, false),
                    (false, false, null) => (null, false),
                    (false, true, false) => (true, false),
                    (false, true, true) => (false, true),
                    (false, true, null) => (null, null),
                    (false, null, false) => (null, false),
                    (false, null, true) => (null, null),
                    (false, null, null) => (null, null),

                    (true, false, false) => (true, false),
                    (true, false, true) => (false, true),
                    (true, false, null) => (null, null),
                    (true, true, false) => (false, true),
                    (true, true, true) => (true, true),
                    (true, true, null) => (null, null),
                    (true, null, false) => (null, null),
                    (true, null, true) => (null, null),
                    (true, null, null) => (null, null),

                    (null, false, false) => (null, false),
                    (null, false, true) => (null, null),
                    (null, false, null) => (null, null),
                    (null, true, false) => (null, null),
                    (null, true, true) => (null, null),
                    (null, true, null) => (null, null),
                    (null, null, false) => (null, null),
                    (null, null, true) => (null, null),
                    (null, null, null) => (null, null),
                };

                sum[i] = s.GetValueOrDefault();
                mask[i] = s.HasValue;
                carry = c;
            }

            SetBits(sum, mask);
        }
    }
}