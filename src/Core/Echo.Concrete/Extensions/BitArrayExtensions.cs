using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Echo.Concrete.Extensions
{
    internal static class BitArrayExtensions
    {
        private class RogueBitArray
        {
            internal int[] Array;

            internal int Length;
        }

        internal static Span<int> AsSpan(this BitArray bitArray)
        {
            var rogue = Unsafe.As<RogueBitArray>(bitArray);
            return rogue.Array.AsSpan();
        }

        internal static Span<byte> AsByteSpan(this BitArray bitArray)
        {
            return MemoryMarshal.AsBytes(bitArray.AsSpan());
        }
    }
}