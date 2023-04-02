using System;

namespace Echo.Memory
{
    internal static class BinaryHelpers
    {
        public static bool All(this Span<byte> span, byte value)
        {
            foreach (byte item in span)
            {
                if (item != value)
                    return false;
            }

            return true;
        }

        public static void Not(this Span<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] = (byte) ~span[i];
        }
        
        public static void And(this Span<byte> span, ReadOnlySpan<byte> other)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] &= other[i];
        }
        
        public static void Or(this Span<byte> span, ReadOnlySpan<byte> other)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] |= other[i];
        }
        
        public static void Xor(this Span<byte> span, ReadOnlySpan<byte> other)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] ^= other[i];
        }
        
        public static unsafe bool SequenceEquals(this Span<byte> span, Span<byte> other)
        {
            // Original code by Hafthor Stefansson
            // Copyright (c) 2008-2013

            if (span == other)
                return true;
            if (span.Length != other.Length)
                return false;

            fixed (byte* p1 = span, p2 = other)
            {
                byte* x1 = p1;
                byte* x2 = p2;
                int length = span.Length;

                for (int i = 0; i < length / sizeof(long); i++, x1 += sizeof(long), x2 += sizeof(long))
                {
                    if (*(long*) x1 != *(long*) x2)
                        return false;
                }

                if ((length & sizeof(int)) != 0)
                {
                    if (*(int*) x1 != *(int*) x2)
                        return false;

                    x1 += sizeof(int);
                    x2 += sizeof(int);
                }

                if ((length & sizeof(short)) != 0)
                {
                    if (*(short*) x1 != *(short*) x2)
                        return false;

                    x1 += sizeof(short);
                    x2 += sizeof(short);
                }

                if ((length & sizeof(byte)) != 0)
                {
                    if (*x1 != *x2)
                        return false;
                }

                return true;
            }
        }

        public static int GetSequenceHashCode(this Span<byte> span)
        {
            unchecked
            {
                int result = 0;
                foreach (byte b in span)
                    result = (result * 31) ^ b;
                return result;
            }
        }

    }
}