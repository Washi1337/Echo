using System;
using Echo.Concrete.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a value on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public interface ICliValue : IConcreteValue
    {
        /// <summary>
        /// Interprets the bits stored in the value as a signed native integer.
        /// </summary>
        /// <param name="is32Bit">Determines whether the native integer is 32 bits or 64 bits wide.</param>
        /// <returns>The signed native integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a native integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a native integer, the same instance is returned.
        /// </remarks>
        NativeIntegerValue InterpretAsI(bool is32Bit);
        
        /// <summary>
        /// Interprets the bits stored in the value as an unsigned native integer.
        /// </summary>
        /// <param name="is32Bit">Determines whether the native integer is 32 bits or 64 bits wide.</param>
        /// <returns>The signed native integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a native integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a native integer, the same instance is returned.
        /// </remarks>
        NativeIntegerValue InterpretAsU(bool is32Bit);
        
        /// <summary>
        /// Interprets the bits stored in the value as a signed 8 bit integer.
        /// </summary>
        /// <returns>The signed 8 bit integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as an 8 bit integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already an 8 bit integer, the same instance is returned.
        /// </remarks>
        I4Value InterpretAsI1();
        
        /// <summary>
        /// Interprets the bits stored in the value as an unsigned 8 bit integer.
        /// </summary>
        /// <returns>The unsigned 8 bit integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as an 8 bit integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already an 8 bit integer, the same instance is returned.
        /// </remarks>
        I4Value InterpretAsU1();
        
        /// <summary>
        /// Interprets the bits stored in the value as a signed 16 bit integer.
        /// </summary>
        /// <returns>The signed 16 bit integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a 16 bit integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a 16 bit integer, the same instance is returned.
        /// </remarks>
        I4Value InterpretAsI2();
        
        /// <summary>
        /// Interprets the bits stored in the value as an unsigned 16 bit integer.
        /// </summary>
        /// <returns>The unsigned 16 bit integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a 16 bit integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a 16 bit integer, the same instance is returned.
        /// </remarks>
        I4Value InterpretAsU2();
        
        /// <summary>
        /// Interprets the bits stored in the value as a signed 32 bit integer.
        /// </summary>
        /// <returns>The signed 32 bit integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a 32 bit integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a 32 bit integer, the same instance is returned.
        /// </remarks>
        I4Value InterpretAsI4();
        
        /// <summary>
        /// Interprets the bits stored in the value as an unsigned 32 bit integer.
        /// </summary>
        /// <returns>The unsigned 32 bit integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a 32 bit integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a 32 bit integer, the same instance is returned.
        /// </remarks>
        I4Value InterpretAsU4();
        
        /// <summary>
        /// Interprets the bits stored in the value as a signed 64 bit integer.
        /// </summary>
        /// <returns>The signed 64 bit integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a 64 bit integer.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a 64 bit integer, the same instance is returned.
        /// </remarks>
        I8Value InterpretAsI8();
        
        /// <summary>
        /// Interprets the bits stored in the value as a 32 bit floating point number.
        /// </summary>
        /// <returns>The 32 bit floating point number.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a 32 bit floating point number.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a 32 bit floating point number, the same instance is returned.
        /// </remarks>
        FValue InterpretAsR4();
        
        /// <summary>
        /// Interprets the bits stored in the value as a 64 bit floating point number.
        /// </summary>
        /// <returns>The 64 floating point number.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as a 64 bit floating point number.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already a 64 bit floating point number, the same instance is returned.
        /// </remarks>
        FValue InterpretAsR8();

        /// <summary>
        /// Interprets the bits stored in the value as an object reference.
        /// </summary>
        /// <param name="is32Bit">Indicates whether the reference to the object should be 32 bits or 64 bits wide.</param>
        /// <returns>The object reference.</returns>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the size of the CLI value is too small for it to be interpretable as an object reference.
        /// </exception>
        /// <remarks>
        /// When this CLI value is already an object reference, the same instance is returned.
        /// </remarks>
        OValue InterpretAsRef(bool is32Bit);

        /// <summary>
        /// Converts the CLI value to a signed native integer.
        /// </summary>
        /// <param name="is32Bit">Indicates whether the native integer should be 32 bits or 64 bits wide.</param>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        NativeIntegerValue ConvertToI(bool is32Bit, bool unsigned, out bool overflowed);

        /// <summary>
        /// Converts the CLI value to an unsigned native integer.
        /// </summary>
        /// <param name="is32Bit">Indicates whether the native integer should be 32 bits or 64 bits wide.</param>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        NativeIntegerValue ConvertToU(bool is32Bit, bool unsigned, out bool overflowed);

        /// <summary>
        /// Converts the CLI value to a signed 8 bit integer.
        /// </summary>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        I4Value ConvertToI1(bool unsigned, out bool overflowed);
        
        /// <summary>
        /// Converts the CLI value to a signed 8 bit integer.
        /// </summary>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        I4Value ConvertToU1(bool unsigned, out bool overflowed);
        
        /// <summary>
        /// Converts the CLI value to a signed 16 bit integer.
        /// </summary>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        I4Value ConvertToI2(bool unsigned, out bool overflowed);
        
        /// <summary>
        /// Converts the CLI value to a signed 16 bit integer.
        /// </summary>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        I4Value ConvertToU2(bool unsigned, out bool overflowed);
        
        /// <summary>
        /// Converts the CLI value to a signed 32 bit integer.
        /// </summary>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        I4Value ConvertToI4(bool unsigned, out bool overflowed);
        
        /// <summary>
        /// Converts the CLI value to a signed 32 bit integer.
        /// </summary>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        I4Value ConvertToU4(bool unsigned, out bool overflowed);
        
        /// <summary>
        /// Converts the CLI value to a signed 64 bit integer.
        /// </summary>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        I8Value ConvertToI8(bool unsigned, out bool overflowed);
        
        /// <summary>
        /// Converts the CLI value to a signed 64 bit integer.
        /// </summary>
        /// <param name="unsigned">Indicates whether the value to convert should be treated as an unsigned number or not.</param>
        /// <param name="overflowed">Indicates the conversion resulted in an overflow.</param>
        /// <returns>The converted value.</returns>
        I8Value ConvertToU8(bool unsigned, out bool overflowed);
        
        /// <summary>
        /// Converts the CLI value to a floating point number.
        /// </summary>
        /// <returns>The converted value.</returns>
        FValue ConvertToR();
    }
}