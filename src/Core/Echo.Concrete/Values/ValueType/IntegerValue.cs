using System;
using System.Text;
using Echo.Core;
using Echo.Core.Values;
using static Echo.Core.TrileanValue;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a primitive integral value that might contain unknown bits.
    /// </summary>
    public abstract class IntegerValue : IValueTypeValue
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
        public virtual Trilean IsZero
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
                            return Trilean.False;
                    }

                    return Trilean.True;
                }


                Span<byte> mask = stackalloc byte[Size];
                GetMask(mask);

                var bitField = new BitField(bits);
                var maskField = new BitField(mask);

                bitField.And(maskField);

                for (int i = 0; i < bits.Length * 8; i++)
                {
                    if (bitField[i])
                        return Trilean.False;
                }

                return Trilean.Unknown;
            }
        }

        /// <inheritdoc />
        public virtual Trilean IsNonZero => !IsZero;

        /// <inheritdoc />
        public Trilean IsPositive => GetLastBit();

        /// <inheritdoc />
        public Trilean IsNegative => !GetLastBit();

        /// <summary>
        /// Gets the most significant bit of the integer value.
        /// </summary>
        /// <returns></returns>
        public virtual Trilean GetLastBit() => GetBit(Size * 8 - 1);

        /// <summary>
        /// Reads a single bit value at the provided index.
        /// </summary>
        /// <param name="index">The index of the bit to read.</param>
        /// <returns>The read bit, or <c>null</c> if the bit value is unknown.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when an invalid index was provided.</exception>
        public abstract Trilean GetBit(int index);

        /// <summary>
        /// Writes a single bit value at the provided index.
        /// </summary>
        /// <param name="index">The index of the bit to write.</param>
        /// <param name="value">The new value of the bit to write. <c>null</c> indicates an unknown bit value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when an invalid index was provided.</exception>
        public abstract void SetBit(int index, Trilean value);

        /// <inheritdoc />
        public abstract void GetBits(Span<byte> buffer);

        /// <inheritdoc />
        public abstract void GetMask(Span<byte> buffer);

        /// <inheritdoc />
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
                Trilean bit = bitString[bitString.Length - i - 1] switch
                {
                    '0' => Trilean.False,
                    '1' => Trilean.True,
                    '?' => Trilean.Unknown,
                    _ => throw new FormatException()
                };

                if (i >= 8 * backingBits.Length)
                {
                    if (bit.IsUnknown || bit)
                        throw new OverflowException();
                }
                else if (bit.IsKnown)
                {
                    bits[i] = bit.ToBooleanOrFalse();
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
                sb.Append(GetBit(i).ToString());

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
                    var result = GetBit(i) & other.GetBit(i);
                    bitField[i] = result.ToBooleanOrFalse();
                    maskField[i] = result.IsKnown;
                }
            }

            SetBits(bits, mask);
        }

        /// <summary>
        /// Performs a bitwise inclusive or operation with another (partially) known integer value.
        /// </summary>
        public virtual void Or(IntegerValue other)
        {
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
                    var result = GetBit(i) | other.GetBit(i);
                    bitField[i] = result.ToBooleanOrFalse();
                    maskField[i] = result.IsKnown;
                }
            }

            SetBits(bits, mask);
        }

        /// <summary>
        /// Performs a bitwise exclusive or operation with another (partially) known integer value.
        /// </summary>
        public virtual void Xor(IntegerValue other)
        {
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
                    var result = GetBit(i) ^ other.GetBit(i);
                    bitField[i] = result.ToBooleanOrFalse();
                    maskField[i] = result.IsKnown;
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

            var sign = signExtend
                ? mask[8 * bitsBuffer.Length - 1]
                    ? bits[8 * bitsBuffer.Length - 1]
                    : Trilean.Unknown
                : Trilean.False;

            count = Math.Min(Size * 8, count);

            for (int i = count; i < bitsBuffer.Length * 8; i++)
            {
                bits[i - count] = bits[i];
                mask[i - count] = mask[i];
            }

            for (int i = 0; i < count; i++)
            {
                bits[8 * bitsBuffer.Length - 1 - i] = sign.ToBooleanOrFalse();
                mask[8 * bitsBuffer.Length - 1 - i] = sign.IsKnown;
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
            // The following implements a ripple full-adder.

            AssertSameBitSize(other);

            Span<byte> sumBuffer = stackalloc byte[Size];
            Span<byte> maskBuffer = stackalloc byte[Size];
            maskBuffer.Fill(0xFF);

            var sum = new BitField(sumBuffer);
            var mask = new BitField(maskBuffer);

            Trilean carry = false;
            for (int i = 0; i < sumBuffer.Length * 8; i++)
            {
                var a = GetBit(i);
                var b = other.GetBit(i);

                // Implement full-adder logic.
                var s = a ^ b ^ carry;
                var c = (carry & (a ^ b)) | (a & b);

                sum[i] = s.ToBooleanOrFalse();
                mask[i] = s.IsKnown;
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
            // The following implements a ripple full-subtractor.

            AssertSameBitSize(other);

            Span<byte> differenceBuffer = stackalloc byte[Size];
            Span<byte> maskBuffer = stackalloc byte[Size];
            maskBuffer.Fill(0xFF);

            var difference = new BitField(differenceBuffer);
            var mask = new BitField(maskBuffer);

            Trilean borrow = false;
            for (int i = 0; i < differenceBuffer.Length * 8; i++)
            {
                var a = GetBit(i);
                var b = other.GetBit(i);

                // Implement full-subtractor logic.
                var d = a ^ b ^ borrow;
                var bOut = (!a & borrow) | (!a & b) | (b & borrow);

                difference[i] = d.ToBooleanOrFalse();
                mask[i] = d.IsKnown;
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
                Trilean bit = other.GetBit(i);

                if (!bit.IsKnown)
                {
                    multipliedByUnknown.LeftShift(i - lastShiftByUnknown);
                    result.Add(multipliedByUnknown);
                    lastShiftByUnknown = i;
                }
                else if (bit.ToBoolean())
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
        /// Divides the current integer with a second (partially) known integer.
        /// </summary>
        /// <param name="other">The integer to divide with</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match or there is dividing by zero.</exception>
        public virtual void Divide(IntegerValue other)
        {
            AssertSameBitSize(other);

            // Throw exception because of dividing by zero
            if (other.IsZero == Trilean.True)
                throw new ArgumentException("Divisor is zero and dividing by zero is not allowed.");

            // There is only one possibility to cover all possible result
            // The solution is to get first number as big as possible by changing all Uknown bits to True
            // And second number needs to be as small as possible by changing all Unknown bits to false

            // First possibility
            var firstNum = (IntegerValue) Copy();
            var secondNum = (IntegerValue) other.Copy();
            var oneNum = (IntegerValue) Copy();
            var result = (IntegerValue) Copy();

            for (int i = 0; i < Size * 8; i++)
            {
                if (firstNum.GetBit(i) == Trilean.Unknown)
                    firstNum.SetBit(i, Trilean.True);

                if (secondNum.GetBit(i) == Trilean.Unknown)
                    secondNum.SetBit(i, Trilean.False);

                oneNum.SetBit(i, Trilean.False);
                result.SetBit(i, Trilean.False);
            }

            oneNum.SetBit(0, Trilean.True);

            // Adding 1 to second number if it is zero
            if (secondNum.IsZero)
                secondNum.Add(oneNum);

            while ((firstNum.IsGreaterThan(secondNum, false) == Trilean.True || firstNum.IsEqualTo(secondNum)))
            {
                result.Add(oneNum);
                firstNum.Subtract(secondNum);
            }

            // Changing all known bits to unknown in greater result 
            if (!IsKnown || !other.IsKnown)
            {
                bool isZeroBit = false;
                for (int i = Size * 8 - 1; i >= 0; i--)
                {
                    // Jumping through zeros
                    if (result.GetBit(i) != Trilean.False || isZeroBit)
                    {
                        isZeroBit = true;
                        result.SetBit(i, Trilean.Unknown);
                    }
                }
            }

            Span<byte> resultBits = stackalloc byte[Size];
            Span<byte> resultMask = stackalloc byte[Size];
            result.GetBits(resultBits);
            result.GetMask(resultMask);

            SetBits(resultBits, resultMask);
        }

        /// <summary>
        /// Divides the current integer with a second (partially) known integer and returns remainder of division. 
        /// </summary>
        /// <param name="other">The integer to divide with</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match or there is dividing by zero.</exception>
        public virtual void Remainder(IntegerValue other)
        {
            AssertSameBitSize(other);

            // Throw exception because of dividing by zero
            if (other.IsZero == Trilean.True)
                throw new ArgumentException("Divisor is zero and dividing by zero is not allowed.");

            // There are 2 possibilities
            // First is that both numbers are known. In this possibility remainder is counted as usual.
            // Second possibility is that some of numbers are unknown.
            // Remainder in second possibility is count as shorter number of this two numbers with all bits set to unknown.

            var firstNum = (IntegerValue) Copy();
            var secondNum = (IntegerValue) other.Copy();
            var result = (IntegerValue) Copy();

            // Possibilities of count
            if (firstNum.IsKnown && secondNum.IsKnown)
            {
                // Remainder is count by classic way
                while (firstNum.IsGreaterThan(secondNum, false) || firstNum.IsEqualTo(secondNum))
                    firstNum.Subtract(secondNum);

                result = firstNum;
            }
            else
            {
                // Setting all unknown bits to True because of finding out smaller number
                for (int i = 0; i < Size * 8; i++)
                {
                    if (firstNum.GetBit(i) == Trilean.Unknown)
                        firstNum.SetBit(i, Trilean.True);

                    if (secondNum.GetBit(i) == Trilean.Unknown)
                        secondNum.SetBit(i, Trilean.True);
                }

                // Result is shorter number
                result = (firstNum.IsGreaterThan(secondNum, false)) 
                    ? (IntegerValue) secondNum.Copy() 
                    : (IntegerValue) firstNum.Copy();

                // Setting all bits to unknown in result
                bool isZeroBit = false;
                for (int i = Size * 8 - 1; i >= 0; i--)
                {
                    // Jumping through zeros
                    if (result.GetBit(i) != Trilean.False || isZeroBit)
                    {
                        isZeroBit = true;
                        result.SetBit(i, Trilean.Unknown);
                    }
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
        public virtual Trilean IsEqualTo(IntegerValue other)
        {
            if (!IsKnown || !other.IsKnown)
            {
                // We are dealing with at least one unknown bit in the bit fields.
                // Conclusion is therefore either false or unknown.

                if (Size != other.Size)
                    return Trilean.False;

                // Check if we definitely know this is not equal to the other.
                // TODO: this could probably use performance improvements.
                for (int i = 0; i < Size * 8; i++)
                {
                    Trilean a = GetBit(i);
                    Trilean b = other.GetBit(i);

                    if (a.IsKnown && b.IsKnown && a.Value != b.Value)
                        return Trilean.False;
                }

                return Trilean.Unknown;
            }

            return Equals(other);
        }

        /// <summary>
        /// Determines whether the integer is greater than the provided integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <param name="signed">Indicates the integers should be interpreted as signed or unsigned integers.</param>
        /// <returns><c>true</c> if the current integer is greater than the provided integer, <c>false</c> if not, and
        /// <c>null</c> if the conclusion of the comparison is not certain.</returns>
        public virtual Trilean IsGreaterThan(IntegerValue other, bool signed)
        {
            if (signed)
            {
                var thisSigned = GetLastBit();
                var otherSigned = other.GetLastBit();
                if (thisSigned.IsUnknown || otherSigned.IsUnknown)
                    return Trilean.Unknown;

                // If the signs do not match, we know the result
                if (thisSigned ^ otherSigned)
                    return otherSigned;

                if (thisSigned)
                    return IsLessThan(other, false);
            }

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
                Trilean a = GetBit(i);
                Trilean b = other.GetBit(i);

                switch (a.Value, b.Value)
                {
                    case (False, True):
                    case (False, Unknown):
                    case (Unknown, True):
                        return Trilean.False;

                    case (True, False):
                        return Trilean.True;

                    case (True, Unknown):
                    case (Unknown, False):
                    case (Unknown, Unknown):
                        return Trilean.Unknown;
                }
            }

            return Trilean.False;
        }

        /// <summary>
        /// Determines whether the integer is less than the provided integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <param name="signed">Indicates the integers should be interpreted as signed or unsigned integers.</param>
        /// <returns><c>true</c> if the current integer is less than the provided integer, <c>false</c> if not, and
        /// <c>null</c> if the conclusion of the comparison is not certain.</returns>
        public virtual Trilean IsLessThan(IntegerValue other, bool signed)
        {
            if (signed)
            {
                var thisSigned = GetLastBit();
                var otherSigned = other.GetLastBit();
                if (thisSigned.IsUnknown || otherSigned.IsUnknown)
                    return Trilean.Unknown;

                // If the signs do not match, we know the result
                if (thisSigned ^ otherSigned)
                    return thisSigned;

                if (thisSigned)
                    return IsGreaterThan(other, false);
            }

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
                Trilean a = GetBit(i);
                Trilean b = other.GetBit(i);

                switch (a.Value, b.Value)
                {
                    case (False, True):
                        return Trilean.True;

                    case (True, False):
                    case (True, Unknown):
                    case (Unknown, False):
                        return Trilean.False;

                    case (False, Unknown):
                    case (Unknown, True):
                        return Trilean.Unknown;
                }
            }

            return Trilean.False;
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
                newBits[i] = bit.ToBooleanOrFalse();
                newMask[i] = bit.IsKnown;
            }

            // Sign extend if necessary.
            if (signExtend)
            {
                Trilean sign = GetBit(bitsToCopy - 1);

                for (int i = bitsToCopy; i < newBitLength; i++)
                {
                    newBits[i] = sign.ToBooleanOrFalse();
                    newMask[i] = sign.IsKnown;
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