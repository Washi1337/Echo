using System;
using System.Text;
using Echo.Core;

namespace Echo.Concrete
{
    public ref struct BitVectorSpan
    {
        [ThreadStatic]
        private static StringBuilder? _builder;
        
        public BitVectorSpan(Span<byte> bits, Span<byte> knownMask)
        {
            if (bits.Length != knownMask.Length)
                throw new ArgumentException("The number of bits is inconsistent.");
                
            Bits = bits;
            KnownMask = knownMask;
        }

        public Trilean this[int index]
        {
            get
            {
                int byteIndex = Math.DivRem(index, 8, out int bitIndex);
                
                if (((KnownMask[byteIndex] >> bitIndex) & 1) == 0)
                    return Trilean.Unknown;

                return ((Bits[byteIndex] >> bitIndex) & 1) == 1;
            }
        }

        public Span<byte> Bits
        {
            get;
        }

        public Span<byte> KnownMask
        {
            get;
        }

        public int Count => Bits.Length * 8;
        
        public BitVectorSpan Slice(int bitIndex)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");

            int byteIndex = bitIndex / 8;
            return new BitVectorSpan(Bits.Slice(byteIndex), KnownMask.Slice(byteIndex));
        }

        public BitVectorSpan Slice(int bitIndex, int length)
        {
            if (bitIndex % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");
            if (length % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "The number of bits in the vector should be a multiple of 8.");

            int byteIndex = bitIndex / 8;
            length /= 8;
            return new BitVectorSpan(Bits.Slice(byteIndex, length), KnownMask.Slice(byteIndex, length));
        }

        public void CopyTo(BitVectorSpan buffer)
        {
            Bits.CopyTo(buffer.Bits);
            KnownMask.CopyTo(buffer.KnownMask);
        }

        public string ToBitString()
        {
            _builder ??= new StringBuilder();
            _builder.Clear();

            for (int i = Count - 1; i >= 0; i--)
            {
                var bit = this[i];
                _builder.Append(bit.ToString());
            }

            return _builder.ToString();
        }

        public string ToHexString()
        {
            _builder ??= new StringBuilder();
            _builder.Clear();

            for (int i = 0; i < Count; i += 4)
            {
                int byteIndex = Math.DivRem(i, 8, out int bitIndex);
                bitIndex = 4 - bitIndex;
                
                if (((KnownMask[byteIndex] >> bitIndex) & 0b1111) != 0b1111)
                {
                    _builder.Append('?');
                }
                else
                {
                    int bits = ((Bits[byteIndex] >> bitIndex) & 0b1111);
                    char hexDigit = bits switch
                    {
                        < 10 => (char) (bits + '0'),
                        < 16 => (char) (bits - 10 + 'A'),
                        _ => throw new ArgumentException("What")
                    };
                    
                    _builder.Append(hexDigit);
                }
            }

            return _builder.ToString();
        }
    }
}