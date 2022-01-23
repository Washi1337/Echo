using System;
using System.Runtime.InteropServices;

namespace Echo.Concrete
{
    public readonly ref partial struct BitVectorSpan
    {
        /// <summary>
        /// Interprets the bit vector as a 32 bit floating point number, and gets or sets the immediate value for it.
        /// </summary>
        public float F32
        {
            get
            {
                var span = MemoryMarshal.Cast<byte, float>(Bits);
                return span[0];
            }
            set
            {
                var span = MemoryMarshal.Cast<byte, float>(Bits);
                span[0] = value;
            }
        }
        
        /// <summary>
        /// Interprets the bit vector as a 64 bit floating point number, and gets or sets the immediate value for it.
        /// </summary>
        public double F64
        {
            get
            {
                var span = MemoryMarshal.Cast<byte, double>(Bits);
                return span[0];
            }
            set
            {
                var span = MemoryMarshal.Cast<byte, double>(Bits);
                span[0] = value;
            }
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
        public void FloatAdd(BitVectorSpan other)
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
        public void FloatSubtract(BitVectorSpan other)
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
                        "Invalid or unsupported bit length for floating point addition.");
            }
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, and multiplies it with another floating point number.
        /// </summary>
        public void FloatMultiply(BitVectorSpan other)
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
                        "Invalid or unsupported bit length for floating point addition.");
            }
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, and divides it with another floating point number.
        /// </summary>
        public void FloatDivide(BitVectorSpan other)
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
                        "Invalid or unsupported bit length for floating point addition.");
            }
        }

        /// <summary>
        /// Interprets the bitvector as a floating point number, divides it with another floating point number and
        /// stores the remainder of the division.
        /// </summary>
        public void FloatRemainder(BitVectorSpan other)
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
                        "Invalid or unsupported bit length for floating point addition.");
            }
        }
        
    }
}