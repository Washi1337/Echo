using System;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides methods for marshalling managed objects into bit vectors and back.
    /// </summary>
    public interface IObjectMarshaller
    {
        /// <summary>
        /// Constructs a bit vector that represents the provided object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The bitvector containing the address to the object.</returns>
        BitVector ToBitVector(object? obj);

        /// <summary>
        /// Interprets the provided bit vector as an object of the provided type.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="targetType">The type to marshal to.</param>
        /// <returns>The marshalled object.</returns>
        object? ToObject(BitVectorSpan vector, Type targetType);
    }

}