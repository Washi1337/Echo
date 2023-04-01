using System;
using System.Runtime.InteropServices;

namespace Echo.Memory
{
    public readonly ref partial struct BitVectorSpan
    {
        /// <summary>
        /// Interprets the bit vector as a 32 bit floating point number, and gets or sets the immediate value for it.
        /// </summary>
        public float F32
        {
            get => MemoryMarshal.Read<float>(Bits);
            set => MemoryMarshal.Write(Bits, ref value);
        }
        
        /// <summary>
        /// Interprets the bit vector as a 64 bit floating point number, and gets or sets the immediate value for it.
        /// </summary>
        public double F64
        {
            get => MemoryMarshal.Read<double>(Bits);
            set => MemoryMarshal.Write(Bits, ref value);
        }

        /// <summary>
        /// Interprets the bit vector as a 32 bit floating point number, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(float value)
        {
            F32 = value;
            KnownMask.Slice(0, sizeof(float)).Fill(0xFF);
        }

        /// <summary>
        /// Interprets the bit vector as a 64 bit floating point number, and writes a fully known immediate value to it. 
        /// </summary>
        /// <param name="value">The value.</param>
        public void Write(double value)
        {
            F64 = value;
            KnownMask.Slice(0, sizeof(double)).Fill(0xFF);
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, and negates it.
        /// </summary>
        public void FloatNegate()
        {
            this[Count - 1] = !this[Count - 1];
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, and adds another floating point number to it.
        /// </summary>
        public void FloatAdd(Memory.BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (!IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: more LLE with unknown bits.
                MarkFullyUnknown();
                return;
            }

            switch (Count)
            {
                case 32:
                    F32 += other.F32;
                    break;
                
                case 64:
                    F64 += other.F64;
                    break;
                
                default:
                    throw new NotSupportedException(
                        "Invalid or unsupported bit length for floating point addition.");
            }
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, and subtracts another floating point number to it.
        /// </summary>
        public void FloatSubtract(Memory.BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (!IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: more LLE with unknown bits.
                MarkFullyUnknown();
                return;
            }

            switch (Count)
            {
                case 32:
                    F32 -= other.F32;
                    break;
                
                case 64:
                    F64 -= other.F64;
                    break;
                
                default:
                    throw new NotSupportedException(
                        "Invalid or unsupported bit length for floating point subtraction.");
            }
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, and multiplies it with another floating point number.
        /// </summary>
        public void FloatMultiply(Memory.BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (!IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: more LLE with unknown bits.
                MarkFullyUnknown();
                return;
            }

            switch (Count)
            {
                case 32:
                    F32 *= other.F32;
                    break;
                
                case 64:
                    F64 *= other.F64;
                    break;
                
                default:
                    throw new NotSupportedException(
                        "Invalid or unsupported bit length for floating point multiplication.");
            }
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, and divides it with another floating point number.
        /// </summary>
        public void FloatDivide(Memory.BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (!IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: more LLE with unknown bits.
                MarkFullyUnknown();
                return;
            }

            switch (Count)
            {
                case 32:
                    F32 /= other.F32;
                    break;
                
                case 64:
                    F64 /= other.F64;
                    break;
                
                default:
                    throw new NotSupportedException(
                        "Invalid or unsupported bit length for floating point division.");
            }
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, divides it with another floating point number and
        /// stores the remainder of the division.
        /// </summary>
        public void FloatRemainder(Memory.BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (!IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: more LLE with unknown bits.
                MarkFullyUnknown();
                return;
            }

            switch (Count)
            {
                case 32:
                    F32 %= other.F32;
                    break;
                
                case 64:
                    F64 %= other.F64;
                    break;
                
                default:
                    throw new NotSupportedException(
                        "Invalid or unsupported bit length for floating point remainder.");
            }
        }

        /// <summary>
        /// interprets the bitvector as a floating point number, and determines whether it is smaller than the provided
        /// floating point bitvector.
        /// </summary>
        /// <param name="other">The other floating point number.</param>
        /// <param name="ordered">A value indicating whether the comparison should be an ordered comparison or not.</param>
        /// <returns>
        /// <see cref="Trilean.True"/> if the current number is less than the provided number,
        /// <see cref="Trilean.False"/> if not, and <see cref="Trilean.Unknown"/> if the conclusion of the comparison
        /// is not certain.
        /// </returns>
        public Trilean FloatIsLessThan(Memory.BitVectorSpan other, bool ordered)
        {
            AssertSameBitSize(other);

            if (!IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: more LLE with unknown bits.
                return Trilean.Unknown;
            }

            return Count switch
            {
                32 => F32 < other.F32,
                64 => F64 < other.F64,
                _ => throw new NotSupportedException("Invalid or unsupported bit length for floating point comparisons.")
            };
        }

        /// <summary>
        /// interprets the bitvector as a floating point number, and determines whether it is greater than the provided
        /// floating point bitvector.
        /// </summary>
        /// <param name="other">The other floating point number.</param>
        /// <param name="ordered">A value indicating whether the comparison should be an ordered comparison or not.</param>
        /// <returns>
        /// <see cref="Trilean.True"/> if the current number is greater than the provided number,
        /// <see cref="Trilean.False"/> if not, and <see cref="Trilean.Unknown"/> if the conclusion of the comparison
        /// is not certain.
        /// </returns>
        public Trilean FloatIsGreaterThan(Memory.BitVectorSpan other, bool ordered)
        {
            AssertSameBitSize(other);

            if (!IsFullyKnown || !other.IsFullyKnown)
            {
                // TODO: more LLE with unknown bits.
                return Trilean.Unknown;
            }

            return Count switch
            {
                32 => F32 > other.F32,
                64 => F64 > other.F64,
                _ => throw new NotSupportedException("Invalid or unsupported bit length for floating point comparisons.")
            };
        }
    }
}