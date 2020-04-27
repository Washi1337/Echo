using System;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a floating point numerical value on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public class FValue : Float64Value, ICliValue
    {
        /// <summary>
        /// Creates a new fully known concrete floating point numerical value.
        /// </summary>
        /// <param name="value">The raw 64 bit value.</param>
        public FValue(double value)
            : base(value)
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