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
    }
}