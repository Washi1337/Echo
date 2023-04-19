
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides extension methods to object marshalling services.
    /// </summary>
    public static class ObjectMarshallerExtensions
    {
        /// <summary>
        /// Constructs an object handle that represents the provided object.
        /// </summary>
        /// <param name="self">The marshaller service.</param>
        /// <param name="obj">The object.</param>
        /// <returns>The handle containing the address to the marshalled object.</returns>
        public static ObjectHandle ToObjectHandle(this IObjectMarshaller self, object? obj)
        {
            return self.ToBitVector(obj).AsObjectHandle(self.Machine);
        }
        
        /// <summary>
        /// Interprets the provided bit vector as an object of the provided type.
        /// </summary>
        /// <param name="self">The marshaller service.</param>
        /// <param name="vector">The vector.</param>
        /// <typeparam name="T">The type to marshal to.</typeparam>
        /// <returns>The marshalled object.</returns>
        public static T? ToObject<T>(this IObjectMarshaller self, BitVectorSpan vector)
        {
            return (T?) self.ToObject(vector, typeof(T));
        }
        
        /// <summary>
        /// Interprets the provided object handle as an object of the provided type.
        /// </summary>
        /// <param name="self">The marshaller service.</param>
        /// <param name="handle">The address.</param>
        /// <typeparam name="T">The type to marshal to.</typeparam>
        /// <returns>The marshalled object.</returns>
        public static T? ToObject<T>(this IObjectMarshaller self, ObjectHandle handle)
        {
            return (T?) self.ToObject(new BitVector(handle.Address), typeof(T));
        }
    }
}