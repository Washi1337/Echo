using System;

namespace Echo.Concrete
{
    internal static class SpanExtensions
    {
        internal static bool All<T>(this Span<T> span, T value)
        {
            foreach (var item in span)
            {
                if (!item.Equals(value))
                    return false;
            }

            return true;
        }
    }
}