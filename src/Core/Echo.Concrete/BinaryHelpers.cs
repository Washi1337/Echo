using System;

namespace Echo.Concrete
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
    }
}