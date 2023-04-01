
namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides extension methods to object marshalling services.
    /// </summary>
    public static class ObjectMarshallerExtensions
    {
        /// <summary>
        /// Interprets the provided bit vector as an object of the provided type.
        /// </summary>
        /// <param name="self">The marshaller service.</param>
        /// <param name="vector">The vector.</param>
        /// <typeparam name="T">The type to marshal to.</typeparam>
        /// <returns>The marshalled object.</returns>
        public static T? ToObject<T>(this IObjectMarshaller self, Memory.BitVectorSpan vector)
        {
            return (T?) self.ToObject(vector, typeof(T));
        }
    }
}