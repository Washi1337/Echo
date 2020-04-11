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
        /// Gets the most significant bit of the integer value.
        /// </summary>
        /// <returns></returns>
        public virtual bool? GetLastBit() => GetBit(Size * 8 - 1);

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
            var mask = new BitArray(Size * 8, true);

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
            // The following implements the following truth table:
            //
            //    | 0 | 1 | ?
            // ---+---+---+---
            //  0 | 0 | 0 | 0
            //  --+---+---+---
            //  1 | 0 | 1 | ?
            //  --+---+---+---
            //  ? | 0 | ? | ?
            
            AssertSameBitSize(other);

            BitArray bits;
            BitArray mask;
            
            if (IsKnown && other.IsKnown)
            {
                // Catch common case where everything is known.
                
                bits = GetBits();
                bits.And(other.GetBits());
                mask = GetMask();
            }
            else
            {
                // Some bits are unknown, perform operation manually.

                bits = new BitArray(Size * 8);
                mask = new BitArray(Size * 8, true);

                for (int i = 0; i < mask.Count; i++)
                {
                    bool? result = (GetBit(i), other.GetBit(i)) switch
                    {
                        (true, true) => true,
                        (true, null) => null,
                        (null, true) => null,
                        (null, null) => null,
                        
                        _ => false,
                    };

                    bits[i] = result.GetValueOrDefault();
                    mask[i] = result.HasValue;
                }
            }

            SetBits(bits, mask);
        }

        /// <summary>
        /// Performs a bitwise inclusive or operation with another (partially) known integer value.
        /// </summary>
        public virtual void Or(IntegerValue other)
        {
            // The following implements the following truth table:
            //
            //    | 0 | 1 | ?
            // ---+---+---+---
            //  0 | 0 | 1 | ?
            //  --+---+---+---
            //  1 | 1 | 1 | 1
            //  --+---+---+---
            //  ? | ? | 1 | ?
            
            AssertSameBitSize(other);

            BitArray bits;
            BitArray mask;
            
            if (IsKnown && other.IsKnown)
            {
                // Catch common case where everything is known.
                
                bits = GetBits();
                bits.Or(other.GetBits());
                mask = GetMask();
            }
            else
            {
                // Some bits are unknown, perform operation manually.
                
                bits = new BitArray(Size * 8);
                mask = new BitArray(Size * 8, true);

                for (int i = 0; i < mask.Count; i++)
                {
                    bool? result = (GetBit(i), other.GetBit(i)) switch
                    {
                        (false, false) => false,
                        (null, false) => null,
                        (null, null) => null,
                        (false, null) => null,
                        
                        _ => true,
                    };

                    bits[i] = result.GetValueOrDefault();
                    mask[i] = result.HasValue;
                }
            }

            SetBits(bits, mask);
        }

        /// <summary>
        /// Performs a bitwise exclusive or operation with another (partially) known integer value.
        /// </summary>
        public virtual void Xor(IntegerValue other)
        {
            // The following implements the following truth table:
            //
            //    | 0 | 1 | ?
            // ---+---+---+---
            //  0 | 0 | 1 | ?
            //  --+---+---+---
            //  1 | 1 | 0 | ?
            //  --+---+---+---
            //  ? | ? | ? | ?
            
            AssertSameBitSize(other);

            BitArray bits;
            BitArray mask;
            
            if (IsKnown && other.IsKnown)
            {
                // Catch common case where everything is known.
                
                bits = GetBits();
                bits.Xor(other.GetBits());
                mask = GetMask();
            }
            else
            {
                // Some bits are unknown, perform operation manually.

                bits = new BitArray(Size * 8);
                mask = new BitArray(Size * 8, true);

                for (int i = 0; i < mask.Count; i++)
                {
                    bool? a = GetBit(i);
                    bool? b = other.GetBit(i);
                    bool? result = a.HasValue && b.HasValue ? a.Value ^ b.Value : (bool?) null;
                    
                    bits[i] = result.GetValueOrDefault();
                    mask[i] = result.HasValue;
                }
            }

            SetBits(bits, mask);
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
            
            for (int i = bits.Count - count - 1; i >= 0; i--)
            {
                bits[i + count] = bits[i];
                mask[i + count] = mask[i];
            }

            for (int i = 0; i < count; i++)
            {
                bits[i] = false;
                mask[i] = true;
            }

            SetBits(bits, mask);
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
            
            for (int i = count; i < bits.Count; i++)
            {
                bits[i - count] = bits[i];
                mask[i - count] = mask[i];
            }

            for (int i = 0; i < count; i++)
            {
                bits[bits.Count - 1 - i] = sign.GetValueOrDefault();
                mask[bits.Count - 1 - i] = sign.HasValue;
            }

            SetBits(bits, mask);
        }

        /// <summary>
        /// Adds a second (partially) known integer to the current integer. 
        /// </summary>
        /// <param name="other">The integer to add.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match.</exception>
        public virtual void Add(IntegerValue other)
        {
            // The following implements a ripple full adder, with unknown bits taken into account. 
            // Essentially, this means that any unknown bit added to another bit results in an unknown sum and/or carry
            // bit.
            
            AssertSameBitSize(other);

            var sum = new BitArray(Size * 8);
            var mask = new BitArray(Size * 8, true);

            bool? carry = false;
            for (int i = 0; i < sum.Length; i++)
            {
                bool? a = GetBit(i);
                bool? b = other.GetBit(i);
                
                // Implement truth table.
                (bool? s, bool? c) = (a, b, carry) switch
                {
                    (false, false, false) => ((bool?) false, (bool?) false),
                    (true, true, true) => (true, true),
                    
                    (true, false, false) => (true, false),
                    (false, true, false) => (true, false),
                    (false, false, true) => (true, false),
                    
                    (null, false, false) => (null, false),
                    (false, null, false) => (null, false),
                    (false, false, null) => (null, false),
                    
                    (false, true, true) => (false, true),
                    (true, false, true) => (false, true),
                    (true, true, false) => (false, true),
                    
                    _ => (null, null),
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

            var difference = new BitArray(Size * 8);
            var mask = new BitArray(Size * 8, true);

            bool? borrow = false;
            for (int i = 0; i < difference.Length; i++)
            {
                bool? a = GetBit(i);
                bool? b = other.GetBit(i);
                
                // Implement truth table.
                (bool? d, bool? bOut) = (a, b, borrow) switch
                {
                    (false, false, false) => ((bool?) false, (bool?) false),
                    
                    (true, true, true) => (true, true),
                    (false, false, true) => (true, true),
                    (false, true, false) => (true, true),
                    
                    (true, false, false) => (true, false),
                    
                    (false, true, true) => (false, true),
                    
                    (true, false, true) => (false, false),
                    (true, true, false) => (false, false),
                    
                    (null, true, true) => (null, true),
                    (false, true, null) => (null, true),
                    
                    (true, false, null) => (null, false),
                    (true, null, false) => (null, false),
                    (null, false, false) => (null, false),
                    
                    _ => (null, null),
                    
                };

                difference[i] = d.GetValueOrDefault();
                mask[i] = d.HasValue;
                borrow = bOut;
            }

            SetBits(difference, mask);
        }

        /// <summary>
        /// Multiplies the current integer with a second (partially) known integer.
        /// </summary>
        /// <param name="other">The integer to multiply with.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match.</exception>
        public virtual void Multiply(IntegerValue other)
        {
            AssertSameBitSize(other);

            // We implement the standard multiplication by adding and shifting, but instead of storing all intermediate
            // results, we can precompute the two possible intermediate results and shift those and add them to an end
            // result to preserve time and memory.
            
            // First possible intermediate result is the current value multiplied by 0. It is redundant to compute this.
            
            // Second possibility is the current value multiplied by 1.
            var multipliedByOne = (IntegerValue) Copy();
            
            // Third possibility is thee current value multiplied by ?. This is effectively marking all set bits unknown.  
            // TODO: We could probably optimise the following operations for the native integer types. 
            var multipliedByUnknown = (IntegerValue) Copy();
            var mask = multipliedByOne.GetMask();
            mask.Not();
            mask.Or(multipliedByOne.GetBits());
            mask.Not();
            multipliedByUnknown.SetBits(multipliedByUnknown.GetBits(), mask);
            
            // Final result.
            var result = new IntegerNValue(Size);

            int lastShiftByOne = 0;
            int lastShiftByUnknown = 0;
            for (int i = 0; i < Size * 8; i++)
            {
                bool? bit = other.GetBit(i);
                
                if (!bit.HasValue)
                {
                    multipliedByUnknown.LeftShift(i - lastShiftByUnknown);
                    result.Add(multipliedByUnknown);
                    lastShiftByUnknown = i;
                }
                else if (bit.Value)
                {
                    multipliedByOne.LeftShift(i - lastShiftByOne);
                    result.Add(multipliedByOne);
                    lastShiftByOne = i;
                }
            }
            
            SetBits(result.Bits, result.Mask);
        }

        /// <summary>
        /// Determines whether the integer is equal to the provided integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns><c>true</c> if the integers are equal, <c>false</c> if not, and
        /// <c>null</c> if the conclusion of the comparison is not certain.</returns>
        public virtual bool? IsEqualTo(IntegerValue other)
        {
            if (!IsKnown)
                return null;
            return Equals(other);
        }

        /// <summary>
        /// Determines whether the integer is greater than the provided integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns><c>true</c> if the current integer is greater than the provided integer, <c>false</c> if not, and
        /// <c>null</c> if the conclusion of the comparison is not certain.</returns>
        public virtual bool? IsGreaterThan(IntegerValue other)
        {
            // The following implements the "truth" table:
            // "-" indicates we do not know the answer yet.
            //
            //    | 0 | 1 | ?
            // ---+---+---+---
            //  0 | - | 0 | 0
            //  --+---+---+---
            //  1 | 1 | - | ?
            //  --+---+---+---
            //  ? | ? | 0 | ?
            
            AssertSameBitSize(other);

            for (int i = Size * 8 - 1; i >= 0; i--)
            {
                bool? a = GetBit(i);
                bool? b = other.GetBit(i);

                switch (a, b)
                {
                    case (false, true):
                    case (false, null):
                    case (null, true):
                        return false;
                    
                    case (true, false):
                        return true;
                    
                    case (true, null):
                    case (null, false):
                    case (null, null):
                        return null;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the integer is less than the provided integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns><c>true</c> if the current integer is less than the provided integer, <c>false</c> if not, and
        /// <c>null</c> if the conclusion of the comparison is not certain.</returns>
        public virtual bool? IsLessThan(IntegerValue other)
        {
            // The following implements the "truth" table:
            // "-" indicates we do not know the answer yet.
            //
            //    | 0 | 1 | ?
            // ---+---+---+---
            //  0 | - | 1 | ?
            //  --+---+---+---
            //  1 | 0 | - | 0
            //  --+---+---+---
            //  ? | 0 | ? | ?
            
            AssertSameBitSize(other);

            for (int i = Size * 8 - 1; i >= 0; i--)
            {
                bool? a = GetBit(i);
                bool? b = other.GetBit(i);

                switch (a, b)
                {
                    case (false, true):
                        return true;
                    
                    case (true, false):
                    case (true, null):
                    case (null, false):
                        return false;
                    
                    case (false, null):
                    case (null, true):
                        return null;
                }
            }

            return false;
        }

        /// <summary>
        /// Extends the integer value to a bigger bit length.
        /// </summary>
        /// <param name="newBitLength">The new bit length to extend the integer to.</param>
        /// <param name="signExtend">Indicates whether the sign bit should be extended.</param>
        /// <returns>
        /// The extended integer. If the provided size is either 8, 16, 32 or 64, this method will return an integer
        /// using the specialized <see cref="Integer8Value"/>, <see cref="Integer16Value"/>, <see cref="Integer32Value"/>
        /// or <see cref="Integer64Value"/> types.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Occurs when the new bit length is shorter than the current bit length.
        /// </exception>
        public virtual IntegerValue Extend(int newBitLength, bool signExtend)
        {
            if (Size * 8 > newBitLength)
                throw new ArgumentException("New bit length is shorter than the current bit length.");

            return Resize(newBitLength, signExtend);
        }

        /// <summary>
        /// Truncates the integer value to a smaller bit length.
        /// </summary>
        /// <param name="newBitLength">The new bit length to truncate the integer to.</param>
        /// <returns>
        /// The truncated integer. If the provided size is either 8, 16, 32 or 64, this method will return an integer
        /// using the specialized <see cref="Integer8Value"/>, <see cref="Integer16Value"/>, <see cref="Integer32Value"/>
        /// or <see cref="Integer64Value"/> types.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Occurs when the new bit length is larger than the current bit length.
        /// </exception>
        public virtual IntegerValue Truncate(int newBitLength)
        {
            if (Size * 8 < newBitLength)
                throw new ArgumentException("New bit length is larger than the current bit length.");

            return Resize(newBitLength, false);
        }

        private IntegerValue Resize(int newBitLength, bool signExtend)
        {
            var newBits = new BitArray(newBitLength);
            var newMask = new BitArray(newBitLength, true);

            // Copy relevant bits.
            int bitsToCopy = Math.Min(newBitLength, Size * 8);
            for (int i = 0; i < bitsToCopy; i++)
            {
                var bit = GetBit(i);
                newBits[i] = bit.GetValueOrDefault();
                newMask[i] = bit.HasValue;
            }

            // Sign extend if necessary.
            if (signExtend)
            {
                bool? sign = GetBit(bitsToCopy - 1);

                for (int i = bitsToCopy; i < newBitLength; i++)
                {
                    newBits[i] = sign.GetValueOrDefault();
                    newMask[i] = sign.HasValue;
                }
            }

            // Optimize to native integer types.
            IntegerValue result = newBitLength switch
            {
                8 => new Integer8Value(0),
                16 => new Integer16Value(0),
                32 => new Integer32Value(0),
                64 => new Integer64Value(0),
                _ => new IntegerNValue(newBitLength / 8)
            };

            // Set contents.
            result.SetBits(newBits, newMask);
            
            return result;
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
                    && BitArrayComparer.Instance.Equals(GetMask(), integerValue.GetMask());
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return BitArrayComparer.Instance.GetHashCode(GetBits())
                   ^ BitArrayComparer.Instance.GetHashCode(GetMask());
        }
    }
}