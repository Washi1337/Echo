using System;
using System.Buffers.Binary;
using static Echo.TrileanValue;

namespace Echo.Memory
{
    public readonly ref partial struct BitVectorSpan
    {
        // Note on performance:
        //
        // For most operations here, LLE of certain operations is relatively slow. We can often avoid having to do
        // full LLE however if the values involved are fully known and are of primitive sizes (e.g. a normal int32).
        // It is computationally cheaper on average to check whether this is the case and then use the native hardware
        // operations, than to always use full LLE on all operations. Hence, this implementation follows this strategy. 

        /// <summary>
        /// Interprets the bit vector as a signed 8 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public sbyte I8
        {
            get => unchecked((sbyte) Bits[0]);
            set => Bits[0] = unchecked((byte) value);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 8 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public byte U8
        {
            get => Bits[0];
            set => Bits[0] = value;
        }
        
        /// <summary>
        /// Interprets the bit vector as a signed 16 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public short I16
        {
            get => BinaryPrimitives.ReadInt16LittleEndian(Bits);
            set => BinaryPrimitives.WriteInt16LittleEndian(Bits, value);
        }
        
        /// <summary>
        /// Interprets the bit vector as an unsigned 16 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public ushort U16
        {
            get => BinaryPrimitives.ReadUInt16LittleEndian(Bits);
            set => BinaryPrimitives.WriteUInt16LittleEndian(Bits, value);
        }
        
        /// <summary>
        /// Interprets the bit vector as a signed 32 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public int I32
        {
            get => BinaryPrimitives.ReadInt32LittleEndian(Bits);
            set => BinaryPrimitives.WriteInt32LittleEndian(Bits, value);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 32 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public uint U32
        {
            get => BinaryPrimitives.ReadUInt32LittleEndian(Bits);
            set => BinaryPrimitives.WriteUInt32LittleEndian(Bits, value);
        }
        
        /// <summary>
        /// Interprets the bit vector as a signed 64 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public long I64
        {
            get => BinaryPrimitives.ReadInt64LittleEndian(Bits);
            set => BinaryPrimitives.WriteInt64LittleEndian(Bits, value);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 64 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public ulong U64
        {
            get => BinaryPrimitives.ReadUInt64LittleEndian(Bits);
            set => BinaryPrimitives.WriteUInt64LittleEndian(Bits, value);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 8 bit integer, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(byte value) => Write(value, 0xFF);

        /// <summary>
        /// Interprets the bit vector as an unsigned 8 bit integer, and writes a partially known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="knownMask">The mask indicating which bits are known.</param>
        public void Write(byte value, byte knownMask)
        {
            U8 = value;
            KnownMask[0] = knownMask;
        }

        /// <summary>
        /// Interprets the bit vector as a signed 8 bit integer, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(sbyte value) => Write(value, 0xFF);

        /// <summary>
        /// Interprets the bit vector as a signed 8 bit integer, and writes a partially known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="knownMask">The mask indicating which bits are known.</param>
        public void Write(sbyte value, byte knownMask)
        {
            I8 = value;
            KnownMask[0] = knownMask;
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 16 bit integer, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(ushort value) => Write(value, 0xFFFF);

        /// <summary>
        /// Interprets the bit vector as an unsigned 16 bit integer, and writes a partially known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="knownMask">The mask indicating which bits are known.</param>
        public void Write(ushort value, ushort knownMask)
        {
            U16 = value;
            BinaryPrimitives.WriteUInt16LittleEndian(KnownMask, knownMask);
        }

        /// <summary>
        /// Interprets the bit vector as a signed 16 bit integer, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(short value) => Write(value, 0xFFFF);

        /// <summary>
        /// Interprets the bit vector as a signed 16 bit integer, and writes a partially known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="knownMask">The mask indicating which bits are known.</param>
        public void Write(short value, ushort knownMask)
        {
            I16 = value;
            BinaryPrimitives.WriteUInt16LittleEndian(KnownMask, knownMask);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 32 bit integer, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(uint value) => Write(value, 0xFFFFFFFF);

        /// <summary>
        /// Interprets the bit vector as an unsigned 32 bit integer, and writes a partially known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="knownMask">The mask indicating which bits are known.</param>
        public void Write(uint value, uint knownMask)
        {
            U32 = value;
            BinaryPrimitives.WriteUInt32LittleEndian(KnownMask, knownMask);
        }

        /// <summary>
        /// Interprets the bit vector as a signed 32 bit integer, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(int value) => Write(value, 0xFFFFFFFF);

        /// <summary>
        /// Interprets the bit vector as a signed 32 bit integer, and writes a partially known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="knownMask">The mask indicating which bits are known.</param>
        public void Write(int value, uint knownMask)
        {
            I32 = value;
            BinaryPrimitives.WriteUInt32LittleEndian(KnownMask, knownMask);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 64 bit integer, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(ulong value) => Write(value, 0xFFFFFFFF_FFFFFFFF);

        /// <summary>
        /// Interprets the bit vector as an unsigned 64 bit integer, and writes a partially known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="knownMask">The mask indicating which bits are known.</param>
        public void Write(ulong value, ulong knownMask)
        {
            U64 = value;
            BinaryPrimitives.WriteUInt64LittleEndian(KnownMask, knownMask);
        }

        /// <summary>
        /// Interprets the bit vector as a signed 64 bit integer, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(long value) => Write(value, 0xFFFFFFFF_FFFFFFFF);

        /// <summary>
        /// Interprets the bit vector as an signed 64 bit integer, and writes a partially known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="knownMask">The mask indicating which bits are known.</param>
        public void Write(long value, ulong knownMask)
        {
            I64 = value;
            BinaryPrimitives.WriteUInt64LittleEndian(KnownMask, knownMask);
        }

        /// <summary>
        /// Writes a fully known native integer into the bit vector at the provided bit index.
        /// </summary>
        /// <param name="value">The native integer to write.</param>
        /// <param name="is32Bit">A value indicating whether the native integer is 32 or 64 bits wide.</param>
        public void WriteNativeInteger(long value, bool is32Bit)
        {
            if (is32Bit)
                Write((int) value);
            else
                Write(value);
        }

        /// <summary>
        /// Interprets the bit vector as an integer, and obtains the most significant bit (MSB) of the bit vector.
        /// </summary>
        public Trilean GetMsb() => this[Count - 1];

        /// <summary>
        /// Interprets the bit vector as an integer and adds a second integer to it. 
        /// </summary>
        /// <param name="other">The integer to add.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        /// <returns>The value of the carry bit after the addition completed.</returns>
        public Trilean IntegerAdd(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerAddLle(other);

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 + other.U8);
                    U8 = new8;
                    return new8 < old8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 + other.U16);
                    U16 = new16;
                    return new16 < old16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 + other.U32;
                    U32 = new32;
                    return new32 < old32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 + other.U64;
                    U64 = new64;
                    return new64 < old64;
                
                default:
                    return IntegerAddLle(other);
            }
        }
        
        private Trilean IntegerAddLle(BitVectorSpan other)
        {
            var carry = Trilean.False;

            for (int i = 0; i < Count; i++)
            {
                var a = this[i];
                var b = other[i];

                // Implement full-adder logic.
                var s = a ^ b ^ carry;
                var c = (carry & (a ^ b)) | (a & b);

                this[i] = s;
                carry = c;
            }

            return carry;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and increments it by one. 
        /// </summary>
        /// <returns>The value of the carry bit after the increment operation completed.</returns>
        public Trilean IntegerIncrement()
        {
            if (Count > 64 || !IsFullyKnown)
                return IntegerIncrementLle();

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 + 1);
                    U8 = new8;
                    return new8 < old8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 + 1);
                    U16 = new16;
                    return new16 < old16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 + 1;
                    U32 = new32;
                    return new32 < old32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 + 1;
                    U64 = new64;
                    return new64 < old64;
                
                default:
                    return IntegerIncrementLle();
            }
        }

        private Trilean IntegerIncrementLle()
        {
            // Optimized version of full-adder that does not require allocation of another vector, and short circuits
            // after carry does not have any effect any more. 

            var carry = Trilean.True;

            for (int i = 0; i < Count && carry != Trilean.False; i++)
            {
                var a = this[i];

                // Implement reduced adder logic.
                var s = a ^ carry;
                var c = carry & a;

                this[i] = s;
                carry = c;
            }

            return carry;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and negates it according to the two's complement semantics.
        /// </summary>
        public void IntegerNegate()
        {
            Not();
            IntegerIncrement();
        }

        /// <summary>
        /// Interprets the bit vector as an integer and subtracts a second integer from it.
        /// </summary>
        /// <param name="other">The integer to subtract.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        /// <returns>The value of the borrow bit after the subtraction completed.</returns>
        public Trilean IntegerSubtract(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerSubtractLle(other);

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 - other.U8);
                    U8 = new8;
                    return new8 > old8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 - other.U16);
                    U16 = new16;
                    return new16 > old16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 - other.U32;
                    U32 = new32;
                    return new32 > old32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 - other.U64;
                    U64 = new64;
                    return new64 > old64;
                
                default:
                    return IntegerSubtractLle(other);
            }
        }

        private Trilean IntegerSubtractLle(BitVectorSpan other)
        {
            var borrow = Trilean.False;

            for (int i = 0; i < Count; i++)
            {
                var a = this[i];
                var b = other[i];

                // Implement full-subtractor logic.
                var d = a ^ b ^ borrow;
                var bOut = (!a & borrow) | (!a & b) | (b & borrow);

                this[i] = d;
                borrow = bOut;
            }

            return borrow;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and decrements it by one. 
        /// </summary>
        /// <returns>The value of the carry bit after the decrement operation completed.</returns>
        public Trilean IntegerDecrement()
        {
            if (Count > 64 || !IsFullyKnown)
                return IntegerDecrementLle();

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 - 1);
                    U8 = new8;
                    return new8 > old8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 -  1);
                    U16 = new16;
                    return new16 > old16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 - 1;
                    U32 = new32;
                    return new32 > old32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 - 1;
                    U64 = new64;
                    return new64 > old64;
                
                default:
                    return IntegerDecrementLle();
            }
        }

        private Trilean IntegerDecrementLle()
        {
            // Optimized version of full-subtractor that does not require allocation of another vector, and short
            // circuits after borrow does not have any effect any more.
            
            var borrow = Trilean.True;

            for (int i = 0; i < Count && borrow != Trilean.False; i++)
            {
                var a = this[i];

                // Implement reduced subtractor logic.
                var d = a ^ borrow;
                var bOut = !a & borrow;

                this[i] = d;
                borrow = bOut;
            }

            return borrow;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and multiplies it by a second integer.
        /// </summary>
        /// <param name="other">The integer to multiply the current integer with.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        /// <returns>A value indicating whether the result was truncated.</returns>
        public Trilean IntegerMultiply(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerMultiplyLle(other);

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 * other.U8);
                    U8 = new8;
                    return other.U8 != 0 && old8 > byte.MaxValue / other.U8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 * other.U16);
                    U16 = new16;
                    return other.U16 != 0 && old16 > ushort.MaxValue / other.U16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 * other.U32;
                    U32 = new32;
                    return other.U32 != 0 && old32 > uint.MaxValue / other.U32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 * other.U64;
                    U64 = new64;
                    return other.U64 != 0 && old64 > ulong.MaxValue / other.U64;
                
                default:
                    return IntegerMultiplyLle(other);
            }
        }

        private Trilean IntegerMultiplyLle(BitVectorSpan other)
        {
            // We implement the standard long multiplication algo by adding and shifting, but instead of storing all
            // intermediate results, we can precompute the two possible intermediate results, shift them, and add them
            // to an end result to preserve time and memory.
            
            // Since there are three possible values in a trilean, there are three possible intermediate results.

            // 1) First possible intermediate result is the current value multiplied by 0. Adding zeroes to a number is
            // equivalent to the identity operation, which means it is redundant to compute this.

            // 2) Second possibility is the current value multiplied by 1.
            var multipliedByOne = GetTemporaryBitVector(0, Count);
            CopyTo(multipliedByOne);

            // 3) Third possibility is thee current value multiplied by ?. This is effectively marking all set bits unknown.
            var multipliedByUnknown = GetTemporaryBitVector(1, Count);
            CopyTo(multipliedByUnknown);

            var mask = multipliedByUnknown.KnownMask;
            mask.Not();
            mask.Or(multipliedByUnknown.Bits);
            mask.Not();

            // Clear all bits, so we can use ourselves as a result bit vector.
            Clear();
            
            var carry = Trilean.False;

            // Perform long multiplication.
            int lastShiftByOne = 0;
            int lastShiftByUnknown = 0;
            for (int i = 0; i < Count; i++)
            {
                var bit = other[i];

                switch (bit.Value)
                {
                    case TrileanValue.False:
                        // Multiply by 0 (Do nothing).
                        break;

                    case TrileanValue.True:
                        // Multiply by 1.
                        multipliedByOne.ShiftLeft(i - lastShiftByOne);
                        carry |= IntegerAdd(multipliedByOne);
                        lastShiftByOne = i;
                        break;
                    
                    case TrileanValue.Unknown:
                        // Multiply by ?.
                        multipliedByUnknown.ShiftLeft(i - lastShiftByUnknown);
                        carry |= IntegerAdd(multipliedByUnknown);
                        lastShiftByUnknown = i;
                        break;
                }
            }
            
            return carry;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and divides it by a second integer.
        /// </summary>
        /// <param name="other">The integer to divide the current integer by.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        public void IntegerDivide(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: LLE div.
                MarkFullyUnknown();
                return;
            }

            switch (Count)
            {
                case 8:
                    U8 /= other.U8;
                    return;

                case 16:
                    U16 /= other.U16;
                    return;

                case 32:
                    U32 /= other.U32;
                    return;
                
                case 64:
                    U64 /= other.U64;
                    return;
                
                default:
                    // TODO: LLE div.
                    MarkFullyUnknown();
                    return;
            }
        }

        /// <summary>
        /// Interprets the bit vector as an integer, divides it by a second integer and produces the remainder.
        /// </summary>
        /// <param name="other">The integer to divide the current integer by.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        public void IntegerRemainder(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: LLE remainder.
                MarkFullyUnknown();
                return;
            }

            switch (Count)
            {
                case 8:
                    U8 %= other.U8;
                    return;

                case 16:
                    U16 %= other.U16;
                    return;

                case 32:
                    U32 %= other.U32;
                    return;
                
                case 64:
                    U64 %= other.U64;
                    return;
                
                default:
                    // TODO: LLE remainder.
                    MarkFullyUnknown();
                    return;
            }
        }

        /// <summary>
        /// Interprets the bit vector as an integer, and determines whether the integer is greater than another integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <param name="signed">Indicates the integers should be interpreted as signed or unsigned integers.</param>
        /// <returns>
        /// <see cref="Trilean.True"/> if the current integer is greater than the provided integer,
        /// <see cref="Trilean.False"/> if not, and <see cref="Trilean.Unknown"/> if the conclusion of the comparison
        /// is not certain.
        /// </returns>
        public Trilean IntegerIsGreaterThan(BitVectorSpan other, bool signed)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerIsGreaterThanLle(other, signed);

            if (signed)
            {
                return Count switch
                {
                    8 => I8 > other.I8,
                    16 => I16 > other.I16,
                    32 => I32 > other.I32,
                    64 => I64 > other.I64,
                    _ => IntegerIsGreaterThanLle(other, signed)
                };
            }

            return Count switch
            {
                8 => U8 > other.U8,
                16 => U16 > other.U16,
                32 => U32 > other.U32,
                64 => U64 > other.U64,
                _ => IntegerIsGreaterThanLle(other, signed)
            };
        }
        
        private Trilean IntegerIsGreaterThanLle(BitVectorSpan other, bool signed)
        {
            if (signed)
            {
                var thisSigned = GetMsb();
                var otherSigned = other.GetMsb();
                if (thisSigned.IsUnknown || otherSigned.IsUnknown)
                    return Trilean.Unknown;

                // If the signs do not match, we know the result
                if (thisSigned ^ otherSigned)
                    return otherSigned;

                if (thisSigned)
                    return IntegerIsLessThan(other, false);
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

            for (int i = Count - 1; i >= 0; i--)
            {
                var a = this[i];
                var b = other[i];

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
        /// Interprets the bit vector as an integer, and determines whether the integer is greater than or equal to
        /// another integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <param name="signed">Indicates the integers should be interpreted as signed or unsigned integers.</param>
        /// <returns>
        /// <see cref="Trilean.True"/> if the current integer is greater than or equal to the provided integer,
        /// <see cref="Trilean.False"/> if not, and <see cref="Trilean.Unknown"/> if the conclusion of the comparison
        /// is not certain.
        /// </returns>
        public Trilean IntegerIsGreaterThanOrEqual(BitVectorSpan other, bool signed)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerIsGreaterThanOrEqualLle(other, signed);

            if (signed)
            {
                return Count switch
                {
                    8 => I8 >= other.I8,
                    16 => I16 >= other.I16,
                    32 => I32 >= other.I32,
                    64 => I64 >= other.I64,
                    _ => IntegerIsGreaterThanOrEqualLle(other, signed)
                };
            }

            return Count switch
            {
                8 => U8 >= other.U8,
                16 => U16 >= other.U16,
                32 => U32 >= other.U32,
                64 => U64 >= other.U64,
                _ => IntegerIsGreaterThanOrEqualLle(other, signed)
            };
        }
        
        private Trilean IntegerIsGreaterThanOrEqualLle(BitVectorSpan other, bool signed)
        {
            if (signed)
            {
                var thisSigned = GetMsb();
                var otherSigned = other.GetMsb();
                if (thisSigned.IsUnknown || otherSigned.IsUnknown)
                    return Trilean.Unknown;

                // If the signs do not match, we know the result
                if (thisSigned ^ otherSigned)
                    return otherSigned;

                if (thisSigned)
                    return IntegerIsLessThanOrEqual(other, false);
            }
            
            AssertSameBitSize(other);

            bool maybeGreater = false;
            bool maybeSmaller = false;
            
            for (int i = Count - 1; i >= 0; i--)
            {
                var a = this[i];
                var b = other[i];

                switch (a.Value, b.Value)
                {
                    case (False, False):
                    case (True, True):
                        // Still inconclusive.
                        break;
                    
                    case (False, True):
                        return maybeGreater ? Trilean.Unknown : Trilean.False;

                    case (True, False):
                        return maybeSmaller ? Trilean.Unknown : Trilean.True;
                    
                    case (True, Unknown):
                    case (Unknown, False):
                        maybeGreater = true;
                        break;
                    
                    case (Unknown, True):
                    case (False, Unknown):
                        maybeSmaller = true;
                        break;

                    case (Unknown, Unknown):
                        return Unknown;
                }
            }

            return maybeSmaller ? Trilean.Unknown : Trilean.True;
        }

        /// <summary>
        /// Interprets the bit vector as an integer, and determines whether the integer is less than another integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <param name="signed">Indicates the integers should be interpreted as signed or unsigned integers.</param>
        /// <returns>
        /// <see cref="Trilean.True"/> if the current integer is less than the provided integer,
        /// <see cref="Trilean.False"/> if not, and <see cref="Trilean.Unknown"/> if the conclusion of the comparison
        /// is not certain.
        /// </returns>
        public Trilean IntegerIsLessThan(BitVectorSpan other, bool signed)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerIsLessThanLle(other, signed);

            if (signed)
            {
                return Count switch
                {
                    8 => I8 < other.I8,
                    16 => I16 < other.I16,
                    32 => I32 < other.I32,
                    64 => I64 < other.I64,
                    _ => IntegerIsLessThanLle(other, signed)
                };
            }

            return Count switch
            {
                8 => U8 < other.U8,
                16 => U16 < other.U16,
                32 => U32 < other.U32,
                64 => U64 < other.U64,
                _ => IntegerIsLessThanLle(other, signed)
            };
        }
        
        private Trilean IntegerIsLessThanLle(BitVectorSpan other, bool signed)
        {
            if (signed)
            {
                var thisSigned = GetMsb();
                var otherSigned = other.GetMsb();
                if (thisSigned.IsUnknown || otherSigned.IsUnknown)
                    return Trilean.Unknown;

                // If the signs do not match, we know the result
                if (thisSigned ^ otherSigned)
                    return thisSigned;

                if (thisSigned)
                    return IntegerIsGreaterThan(other, false);
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

            for (int i = Count - 1; i >= 0; i--)
            {
                var a = this[i];
                var b = other[i];

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
        /// Interprets the bit vector as an integer, and determines whether the integer is less than or equal to
        /// another integer.
        /// </summary>
        /// <param name="other">The other integer.</param>
        /// <param name="signed">Indicates the integers should be interpreted as signed or unsigned integers.</param>
        /// <returns>
        /// <see cref="Trilean.True"/> if the current integer is greater than or equal to the provided integer,
        /// <see cref="Trilean.False"/> if not, and <see cref="Trilean.Unknown"/> if the conclusion of the comparison
        /// is not certain.
        /// </returns>
        public Trilean IntegerIsLessThanOrEqual(BitVectorSpan other, bool signed)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerIsLessThanOrEqualLle(other, signed);

            if (signed)
            {
                return Count switch
                {
                    8 => I8 <= other.I8,
                    16 => I16 <= other.I16,
                    32 => I32 <= other.I32,
                    64 => I64 <= other.I64,
                    _ => IntegerIsLessThanOrEqualLle(other, signed)
                };
            }

            return Count switch
            {
                8 => U8 <= other.U8,
                16 => U16 <= other.U16,
                32 => U32 <= other.U32,
                64 => U64 <= other.U64,
                _ => IntegerIsLessThanOrEqualLle(other, signed)
            };
        }
        
        private Trilean IntegerIsLessThanOrEqualLle(BitVectorSpan other, bool signed)
        {
            if (signed)
            {
                var thisSigned = GetMsb();
                var otherSigned = other.GetMsb();
                if (thisSigned.IsUnknown || otherSigned.IsUnknown)
                    return Trilean.Unknown;

                // If the signs do not match, we know the result
                if (thisSigned ^ otherSigned)
                    return thisSigned;

                if (thisSigned)
                    return IntegerIsGreaterThanOrEqual(other, false);
            }
            
            AssertSameBitSize(other);

            bool maybeGreater = false;
            bool maybeSmaller = false;
            
            for (int i = Count - 1; i >= 0; i--)
            {
                var a = this[i];
                var b = other[i];

                switch (a.Value, b.Value)
                {
                    case (False, False):
                    case (True, True):
                        // Still inconclusive.
                        break;
                    
                    case (False, True):
                        return maybeGreater ? Trilean.Unknown : Trilean.True;

                    case (True, False):
                        return maybeSmaller ? Trilean.Unknown : Trilean.False;
                    
                    case (True, Unknown):
                    case (Unknown, False):
                        maybeGreater = true;
                        break;
                    
                    case (False, Unknown):
                    case (Unknown, True):
                        maybeSmaller = true;
                        break;

                    case (Unknown, Unknown):
                        return Unknown;
                }
            }

            return maybeGreater ? Trilean.Unknown : Trilean.True;
        }
    }
}