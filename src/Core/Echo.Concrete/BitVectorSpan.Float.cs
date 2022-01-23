using System;
using System.Runtime.InteropServices;

namespace Echo.Concrete
{
    public readonly ref partial struct BitVectorSpan
    {
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

        public void FloatNegate()
        {
            this[Count - 1] = !this[Count - 1];
        }

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