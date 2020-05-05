using System;
using System.Collections;
using System.Text;
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

        /// <inheritdoc />
        public virtual bool? IsZero
        {
            get
            {
                Span<byte> bits = stackalloc byte[Size];
                GetBits(bits);

                if (IsKnown)
                {
                    for (var i = 0; i < Size; i++)
                    {
                        if (bits[i] != 0)
                        {
                            return false;
                        }
                    }

                    return true;
                }


                Span<byte> mask = stackalloc byte[Size];
                GetMask(mask);
                
                var bitField = new BitField(bits);
                var maskField = new BitField(mask);
                
                bitField.And(maskField);
                
                for (int i = 0; i < bits.Length * 8; i++)
                {
                    if (bitField[i])
                    {
                        return false;
                    }
                }

                return null;
            }
        }

        /// <inheritdoc />
        public virtual bool? IsNonZero => !IsZero;

        /// <inheritdoc />
        public bool? IsPositive => GetLastBit();

        /// <inheritdoc />
        public bool? IsNegative => !GetLastBit();

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
        /// <param name="buffer">The buffer to write the raw bits to.</param>
        /// <remarks>
        /// The bits returned by this method assume the value is known entirely. Any bit that is marked unknown will be
        /// set to 0. 
        /// </remarks>
        public abstract void GetBits(Span<byte> buffer);

        /// <summary>
        /// Gets the bit mask indicating the bits that are known.  
        /// </summary>
        /// <param name="buffer">The buffer to write the raw mask to.</param>
        /// <remarks>
        /// If bit at location <c>i</c> equals 1, bit <c>i</c> is known, and unknown otherwise.
        /// </remarks>
        public abstract void GetMask(Span<byte> buffer);

        /// <summary>
        /// Replaces the raw contents of the integer with the provided bits and known mask.
        /// </summary>
        /// <param name="bits">The new bit values.</param>
        /// <param name="mask">The new bit mask indicating the known bits.</param>
        public abstract void SetBits(Span<byte> bits, Span<byte> mask);

        /// <summary>
        /// Parses a (partially) known bit string and sets the contents of integer to the parsed result.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        /// <exception cref="OverflowException"></exception>
        public void SetBits(string bitString)
        {
            Span<byte> backingBits = stackalloc byte[Size];
            Span<byte> backingMask = stackalloc byte[Size];
            backingMask.Fill(0xFF);
            
            var bits = new BitField(backingBits);
            var mask = new BitField(backingMask);

            for (int i = 0; i < bitString.Length; i++)
            {
                bool? bit = bitString[bitString.Length - i - 1] switch
                {
                    '0' => false,
                    '1' => true,
                    '?' => null,
                    _ => throw new FormatException()
                };

                if (i >= 8 * backingBits.Length)
                {
                    if (!bit.HasValue || bit.Value)
                        throw new OverflowException();
                }
                else if (bit.HasValue)
                {
                    bits[i] = bit.Value;
                }
                else
                {
                    mask[i] = false;
                }
            }

            SetBits(backingBits, backingMask);
        } 

        /// <inheritdoc />
        public override string ToString()
        {
            var size = Size * 8;
            var sb = new StringBuilder(size);
            for (int i = size - 1; i >= 0; i--)
            {
                var bit = GetBit(i);
                if (bit.HasValue)
                {
                    sb.Append(bit.Value ? '1' : '0');
                }
                else
                {
                    sb.Append('?');
                }
            }
            
            return sb.ToString();
        }

        /// <inheritdoc />
        public abstract IValue Copy();

        /// <summary>
        /// Performs a bitwise not operation on the (partially) known integer value.
        /// </summary>
        public virtual void Not()
        {
            Span<byte> bitsBuffer = stackalloc byte[Size];
            Span<byte> maskBuffer = stackalloc byte[Size];
            
            GetBits(bitsBuffer);
            GetMask(maskBuffer);
            
            var bits = new BitField(bitsBuffer);
            var mask = new BitField(maskBuffer);
            
            bits.Not();
            bits.And(mask);

            SetBits(bitsBuffer, maskBuffer);
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

            Span<byte> bits = stackalloc byte[Size];
            GetBits(bits);
            
            Span<byte> mask = stackalloc byte[Size];
            GetMask(mask);
            
            if (IsKnown && other.IsKnown)
            {
                // Catch common case where everything is known.

                Span<byte> otherBits = stackalloc byte[Size];
                other.GetBits(otherBits);
                
                new BitField(bits).And(new BitField(otherBits));
            }
            else
            {
                // Some bits are unknown, perform operation manually.
                
                var bitField = new BitField(bits);
                var maskField = new BitField(mask);

                for (int i = 0; i < mask.Length * 8; i++)
                {
                    bool? result = (GetBit(i), other.GetBit(i)) switch
                    {
                        (true, true) => true,
                        (true, null) => null,
                        (null, true) => null,
                        (null, null) => null,
                        
                        _ => false,
                    };

                    bitField[i] = result.GetValueOrDefault();
                    maskField[i] = result.HasValue;
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

            Span<byte> bits = stackalloc byte[Size];
            GetBits(bits);
            
            Span<byte> mask = stackalloc byte[Size];
            GetMask(mask);
            
            if (IsKnown && other.IsKnown)
            {
                // Catch common case where everything is known.

                Span<byte> otherBits = stackalloc byte[Size];
                other.GetBits(otherBits);
                
                new BitField(bits).Or(new BitField(otherBits));
            }
            else
            {
                // Some bits are unknown, perform operation manually.
                
                var bitField = new BitField(bits);
                var maskField = new BitField(mask);
                
                for (int i = 0; i < mask.Length * 8; i++)
                {
                    bool? result = (GetBit(i), other.GetBit(i)) switch
                    {
                        (false, false) => false,
                        (null, false) => null,
                        (null, null) => null,
                        (false, null) => null,
                        
                        _ => true,
                    };

                    bitField[i] = result.GetValueOrDefault();
                    maskField[i] = result.HasValue;
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

            Span<byte> bits = stackalloc byte[Size];
            GetBits(bits);
            
            Span<byte> mask = stackalloc byte[Size];
            GetMask(mask);
            
            if (IsKnown && other.IsKnown)
            {
                // Catch common case where everything is known.

                Span<byte> otherBits = stackalloc byte[Size];
                other.GetBits(otherBits);
                
                new BitField(bits).Xor(new BitField(otherBits));
            }
            else
            {
                // Some bits are unknown, perform operation manually.
                
                var bitField = new BitField(bits);
                var maskField = new BitField(mask);

                for (int i = 0; i < mask.Length * 8; i++)
                {
                    bool? a = GetBit(i);
                    bool? b = other.GetBit(i);
                    bool? result = a.HasValue && b.HasValue ? a.Value ^ b.Value : (bool?) null;
                    
                    bitField[i] = result.GetValueOrDefault();
                    maskField[i] = result.HasValue;
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

            Span<byte> bitsBuffer = stackalloc byte[Size];
            GetBits(bitsBuffer);
            
            Span<byte> maskBuffer = stackalloc byte[Size];
            GetMask(maskBuffer);
            
            var bits = new BitField(bitsBuffer);
            var mask = new BitField(maskBuffer);

            count = Math.Min(Size * 8, count);
            
            for (int i = 8 * bitsBuffer.Length - count - 1; i >= 0; i--)
            {
                bits[i + count] = bits[i];
                mask[i + count] = mask[i];
            }

            for (int i = 0; i < count; i++)
            {
                bits[i] = false;
                mask[i] = true;
            }

            SetBits(bitsBuffer, maskBuffer);
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

            Span<byte> bitsBuffer = stackalloc byte[Size];
            GetBits(bitsBuffer);
            
            Span<byte> maskBuffer = stackalloc byte[Size];
            GetMask(maskBuffer);
            
            var bits = new BitField(bitsBuffer);
            var mask = new BitField(maskBuffer);

            bool? sign = signExtend 
                ? mask[8 * bitsBuffer.Length - 1] ? bits[8 * bitsBuffer.Length - 1] : (bool?) null 
                : false;
            
            count = Math.Min(Size * 8, count);
            
            for (int i = count; i < bitsBuffer.Length * 8; i++)
            {
                bits[i - count] = bits[i];
                mask[i - count] = mask[i];
            }

            for (int i = 0; i < count; i++)
            {
                bits[8 * bitsBuffer.Length - 1 - i] = sign.GetValueOrDefault();
                mask[8 * bitsBuffer.Length - 1 - i] = sign.HasValue;
            }

            SetBits(bitsBuffer, maskBuffer);
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

            Span<byte> sumBuffer = stackalloc byte[Size];
            Span<byte> maskBuffer = stackalloc byte[Size];
            maskBuffer.Fill(0xFF);
            
            var sum = new BitField(sumBuffer);
            var mask = new BitField(maskBuffer);

            bool? carry = false;
            for (int i = 0; i < sumBuffer.Length * 8; i++)
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

            SetBits(sumBuffer, maskBuffer);
        }

        /// <summary>
        /// Transforms the (partially) known integer into its twos complement.
        /// </summary>
        public virtual void TwosComplement()
        {
            Not();
            Span<byte> buffer = stackalloc byte[Size];
            new BitField(buffer) { [0] = true };

            Add(new IntegerNValue(buffer));
        }

        /// <summary>
        /// Subtracts a second (partially) known integer from the current integer. 
        /// </summary>
        /// <param name="other">The integer to subtract.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match.</exception>
        public virtual void Subtract(IntegerValue other)
        {
            AssertSameBitSize(other);

            Span<byte> differenceBuffer = stackalloc byte[Size];
            Span<byte> maskBuffer = stackalloc byte[Size];
            maskBuffer.Fill(0xFF);
            
            var difference = new BitField(differenceBuffer);
            var mask = new BitField(maskBuffer);

            bool? borrow = false;
            for (int i = 0; i < differenceBuffer.Length * 8; i++)
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

            SetBits(differenceBuffer, maskBuffer);
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

            Span<byte> maskBuffer = stackalloc byte[Size];
            multipliedByOne.GetMask(maskBuffer);

            Span<byte> bitsBuffer = stackalloc byte[Size];
            multipliedByOne.GetBits(bitsBuffer);
            
            var mask = new BitField(maskBuffer);
            var bits = new BitField(bitsBuffer);
            
            mask.Not();
            mask.Or(bits);
            mask.Not();

            Span<byte> unknownBits = stackalloc byte[Size];
            multipliedByUnknown.GetBits(unknownBits);
            
            multipliedByUnknown.SetBits(unknownBits, maskBuffer);
            
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

            Span<byte> resultBits = stackalloc byte[Size];
            Span<byte> resultMask = stackalloc byte[Size];
            result.GetBits(resultBits);
            result.GetMask(resultMask);
            
            SetBits(resultBits, resultMask);
        }

        /// <summary>
        /// Determines whether the integer is equal to the provided integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <returns><c>true</c> if the integers are equal, <c>false</c> if not, and
        /// <c>null</c> if the conclusion of the comparison is not certain.</returns>
        public virtual bool? IsEqualTo(IntegerValue other)
        {
            if (!IsKnown || !other.IsKnown)
            {
                // We are dealing with at least one unknown bit in the bit fields.
                // Conclusion is therefore either false or unknown.
                
                if (Size != other.Size)
                    return false;

                // Check if we definitely know this is not equal to the other.
                // TODO: this could probably use performance improvements.
                for (int i = 0; i < Size * 8; i++)
                {
                    bool? a = GetBit(i);
                    bool? b = other.GetBit(i);

                    if (a.HasValue && b.HasValue && a.Value != b.Value)
                        return false;
                }

                return null;
            }
            
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
            Span<byte> newBitsBuffer = stackalloc byte[newBitLength / 8];
            Span<byte> newMaskBuffer = stackalloc byte[newBitLength / 8];
            newMaskBuffer.Fill(0xFF);
            
            var newBits = new BitField(newBitsBuffer);
            var newMask = new BitField(newMaskBuffer);

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
            result.SetBits(newBitsBuffer, newMaskBuffer);
            
            return result;
        }

        /// <summary>
        /// Marks all bits in the integer value as unknown.
        /// </summary>
        public abstract void MarkFullyUnknown();

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

                Span<byte> one = stackalloc byte[Size];
                Span<byte> two = stackalloc byte[Size];
                
                GetBits(one);
                integerValue.GetBits(two);

                if (new BitField(one).Equals(new BitField(two)) == false)
                {
                    return false;
                }
                
                GetMask(one);
                integerValue.GetMask(two);
                
                return new BitField(one).Equals(new BitField(two));
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            Span<byte> bitsBuffer = stackalloc byte[Size];
            Span<byte> maskBuffer = stackalloc byte[Size];
            GetBits(bitsBuffer);
            GetMask(maskBuffer);
            
            return new BitField(bitsBuffer).GetHashCode() ^ new BitField(maskBuffer).GetHashCode();
        }
    }
}