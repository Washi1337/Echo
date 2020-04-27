using System;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a 32 bit integer value on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public class I4Value : Integer32Value, ICliValue
    {
        /// <summary>
        /// Creates a new, fully known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        public I4Value(int value)
            : base(value)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public I4Value(int value, uint mask)
            : base(value, mask)
        {
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 32 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        public I4Value(string bitString)
            : base(bitString)
        {
        }

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsI(bool is32Bit)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsU(bool is32Bit)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value InterpretAsI1()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value InterpretAsU1()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value InterpretAsI2()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value InterpretAsU2()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value InterpretAsI4()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value InterpretAsU4()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I8Value InterpretAsI8()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public FValue InterpretAsR4()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public FValue InterpretAsR8()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public OValue InterpretAsRef()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public NativeIntegerValue ConvertToI(bool is32Bit, bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public NativeIntegerValue ConvertToU(bool is32Bit, bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value ConvertToI1(bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value ConvertToU1(bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value ConvertToI2(bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value ConvertToU2(bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value ConvertToI4(bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I4Value ConvertToU4(bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I8Value ConvertToI8(bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public I8Value ConvertToU8(bool unsigned, out bool overflowed)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public FValue ConvertToR()
        {
            throw new NotImplementedException();
        }
    }
}