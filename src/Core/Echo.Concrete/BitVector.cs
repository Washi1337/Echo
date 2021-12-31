using System;

namespace Echo.Concrete
{
    public class BitVector
    {
        public BitVector(int count, bool initialize)
        {
            if (count % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(count), "The number of bits in the vector should be a multiple of 8.");

            Bits = new byte[count / 8];
            KnownMask = new byte[Bits.Length];

            if (initialize)
                KnownMask.AsSpan().Fill(0xFF);
        }
        
        public BitVector(byte[] bits)
        {
            Bits = bits;
            KnownMask = new byte[bits.Length];
            KnownMask.AsSpan().Fill(0xFF);
        }
        
        public BitVector(byte[] bits, byte[] knownMask)
        {
            Bits = bits;
            KnownMask = knownMask;
        }

        public byte[] Bits
        {
            get;
        }

        public byte[] KnownMask
        {
            get;
        }
        
        public int Count => Bits.Length * 8;

        public BitVectorSpan AsSpan() => new(Bits, KnownMask);
        
        public BitVectorSpan AsSpan(int bitIndex)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");

            int byteIndex = bitIndex / 8;
            return new BitVectorSpan(Bits.AsSpan(byteIndex), KnownMask.AsSpan(byteIndex));
        }

        public BitVectorSpan AsSpan(int bitIndex, int length)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");
            if (length % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");

            int byteIndex = bitIndex / 8;
            length /= 8;
            return new BitVectorSpan(Bits.AsSpan(byteIndex, length), KnownMask.AsSpan(byteIndex, length));
        }
    }
}