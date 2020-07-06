using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents an array-like value that can be used in the context of executing CIL code. 
    /// </summary>
    public interface IDotNetArrayValue : IDotNetValue
    {
        /// <summary>
        /// Gets the length of the array structure.
        /// </summary>
        int Length
        {
            get;
        }

        /// <summary>
        /// Loads an element of the array as a native sized integer.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        NativeIntegerValue LoadElementI(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as a signed 8 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        I4Value LoadElementI1(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as a signed 16 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        I4Value LoadElementI2(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as a signed 32 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        I4Value LoadElementI4(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as a signed 64 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        I8Value LoadElementI8(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as an unsigned 8 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        I4Value LoadElementU1(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as an unsigned 16 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        I4Value LoadElementU2(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as an unsigned 32 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        I4Value LoadElementU4(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as a 32 bit floating point number.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        FValue LoadElementR4(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as a 64 bit floating point number.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        FValue LoadElementR8(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Loads an element of the array as an object reference.
        /// </summary>
        /// <param name="index">The index of the element to read.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CLI operates on.</param>
        /// <returns>The element.</returns>
        OValue LoadElementRef(int index, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided native sized integer.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        public void StoreElementI(int index, NativeIntegerValue value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided signed 8 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementI1(int index, I4Value value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided signed 16 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementI2(int index, I4Value value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided signed 32 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementI4(int index, I4Value value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided signed 64 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementI8(int index, I8Value value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided unsigned 8 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementU1(int index, I4Value value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided unsigned 16 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementU2(int index, I4Value value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided unsigned 32 bit integer.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementU4(int index, I4Value value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided 32 bit floating point number
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementR4(int index, FValue value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided 64 bit floating point number.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementR8(int index, FValue value, ICliMarshaller marshaller);

        /// <summary>
        /// Replaces an element in the array with the provided object reference.
        /// </summary>
        /// <param name="index">The index of the element to replace.</param>
        /// <param name="value">The value to replace the element with.</param>
        /// <param name="marshaller">The marshaller to use for converting the raw value to a value the CTS operates on.</param>
        void StoreElementRef(int index, OValue value, ICliMarshaller marshaller);
    }
}