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
        /// Determines whether the integer consists of only zeroes.
        /// </summary>
        /// <remarks>
        /// If this value is <c>null</c>, it is unknown whether this value contains only zeroes.
        /// </remarks>
        public virtual bool? IsZero
        {
            get
            {
                var bits = GetBits();
                
                if (IsKnown)
                    return BitArrayComparer.Instance.Equals(bits, new BitArray(bits.Count, false));;

                var mask = GetMask();
                bits.And(mask);
                
                var raw = new int[bits.Count / sizeof(int)];
                bits.CopyTo(raw, 0);
                
                for (int i = 0; i < raw.Length; i++)
                {
                    if (raw[i] != 0)
                        return false;
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether the integer contains at least a single one in its bit string.
        /// </summary>
        /// <remarks>
        /// If this value is <c>null</c>, it is unknown whether this value contains at least a single one in its bit string.
        /// </remarks>
        public virtual bool? IsNonZero => !IsZero;

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

        /// <summary>
        /// Parses a (partially) known bit string and sets the contents of integer to the parsed result.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        /// <exception cref="OverflowException"></exception>
        public void SetBits(string bitString)
        {
            var bits = new BitArray(Size * 8);
            var mask = new BitArray(Size * 8);

            for (int i = 0; i < bitString.Length; i++)
            {
                bool? bit = bitString[bitString.Length - i - 1] switch
                {
                    '0' => false,
                    '1' => true,
                    '?' => null,
                    _ => throw new FormatException()
                };

                if (i >= bits.Count)
                {
                    if (!bit.HasValue || bit.Value)
                        throw new OverflowException();
                }
                else if (bit.HasValue)
                {
                    bits[i] = bit.Value;
                    mask[i] = true;
                }
                else
                {
                    mask[i] = false;
                }
            }

            SetBits(bits, mask);
        } 

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
        /// Performs a bitwise not operation on the (partially) known integer value.
        /// </summary>
        public virtual void Not()
        {
            var bits = GetBits();
            var mask = GetMask();
            
            bits.Not();
            bits.And(mask);

            SetBits(bits, mask);
        }

        /// <summary>
        /// Performs a bitwise and operation with another (partially) known integer value.
        /// </summary>
        public virtual void And(IntegerValue other)
        {
            AssertSameBitSize(other);

            var bits = GetBits();
            bits.And(other.GetBits());

            SetBits(bits, CombineKnownMasks(other));
        }

        /// <summary>
        /// Performs a bitwise inclusive or operation with another (partially) known integer value.
        /// </summary>
        public virtual void Or(IntegerValue other)
        {
            AssertSameBitSize(other);
            
            var bits = GetBits();
            bits.Or(other.GetBits());

            SetBits(bits, CombineKnownMasks(other));
        }

        /// <summary>
        /// Performs a bitwise exclusive or operation with another (partially) known integer value.
        /// </summary>
        public virtual void Xor(IntegerValue other)
        {
            AssertSameBitSize(other);
            
            var bits = GetBits();
            bits.Xor(other.GetBits());

            SetBits(bits, CombineKnownMasks(other));
        }
        
        private BitArray CombineKnownMasks(IntegerValue other)
        {
            var mask = GetMask();
            if (!IsKnown || !other.IsKnown)
            {
                var otherMask = other.GetMask();

                mask.Not();
                otherMask.Not();

                mask.Or(otherMask);
                mask.Not();
            }

            return mask;
        }

        /// <summary>
        /// Performs a bitwise shift to the left on this (partially) known integer.
        /// </summary>
        /// <param name="count">The number of bits to shift.</param>
        public virtual void LeftShift(int count)
        {
            if (count == 0)
                return;
            
            var bits = GetBits();
            var mask = GetMask();

            count = Math.Min(Size * 8, count);
            
            for (int i = 0; i < bits.Count - count; i++)
            {
                bits[i + count] = bits[i];
                mask[i + count] = mask[i];
            }

            for (int i = 0; i < count; i++)
            {
                bits[i] = false;
                mask[i] = true;
            }
        }

        /// <summary>
        /// Performs a bitwise shift to the right on this (partially) known integer.
        /// </summary>
        /// <param name="count">The number of bits to shift.</param>
        /// <param name="signExtend">Indicates whether the sign bit should be extended or should always be filled
        /// in with zeroes.</param> 
        public virtual void RightShift(int count, bool signExtend)
        {
            if (count == 0)
                return;
            
            var bits = GetBits();
            var mask = GetMask();

            bool? sign = signExtend 
                ? mask[bits.Count - 1] ? bits[bits.Count - 1] : (bool?) null 
                : false;
            
            count = Math.Min(Size * 8, count);
            
            for (int i = bits.Count - 1; i >= count; i--)
            {
                bits[i - count] = bits[i];
                mask[i - count] = mask[i];
            }

            for (int i = 0; i < count; i++)
            {
                bits[bits.Count - 1 - i] = !sign.HasValue || sign.Value;
                mask[bits.Count - 1 - i] = sign.HasValue;
            }
        }

        /// <summary>
        /// Adds a second (partially) known integer to the current integer. 
        /// </summary>
        /// <param name="other">The integer to add.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match.</exception>
        public virtual void Add(IntegerValue other)
        {
            AssertSameBitSize(other);

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

        /// <summary>
        /// Transforms the (partially) known integer into its twos complement.
        /// </summary>
        public virtual void TwosComplement()
        {
            Not();
            var one = new BitArray(Size * 8);
            one[0] = true;
            Add(new IntegerNValue(one));
        }

        /// <summary>
        /// Subtracts a second (partially) known integer from the current integer. 
        /// </summary>
        /// <param name="other">The integer to subtract.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match.</exception>
        public virtual void Subtract(IntegerValue other)
        {
            AssertSameBitSize(other);
            
            other = (IntegerValue) other.Copy();
            other.TwosComplement();
            Add(other);
        }

        private void AssertSameBitSize(IntegerValue other)
        {
            if (Size != other.Size)
                throw new ArgumentException($"Cannot perform a binary operation on a {other.Size * 8} bit integer and a {Size * 8} bit integer.");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is IntegerValue integerValue)
            {
                if (Size != integerValue.Size)
                    return false;

                return BitArrayComparer.Instance.Equals(GetBits(), integerValue.GetBits())
                    || BitArrayComparer.Instance.Equals(GetMask(), integerValue.GetMask());
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return BitArrayComparer.Instance.GetHashCode(GetBits())
                   ^ BitArrayComparer.Instance.GetHashCode(GetBits());
        }
    }
}