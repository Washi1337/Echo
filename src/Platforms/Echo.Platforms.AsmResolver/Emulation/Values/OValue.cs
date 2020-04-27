using System;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents an object reference on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public class OValue : ICliValue
    {
        private readonly bool? _isNull;

        /// <summary>
        /// Creates a new object reference value.
        /// </summary>
        /// <param name="isNull">Indicates whether the value is null or not.</param>
        public OValue(bool? isNull)
        {
            _isNull = isNull;
        }

        /// <inheritdoc />
        public bool IsKnown => _isNull.HasValue;

        /// <inheritdoc />
        public int Size => 4;

        /// <inheritdoc />
        public bool IsValueType => false;

        /// <inheritdoc />
        public bool? IsZero => _isNull;

        /// <inheritdoc />
        public bool? IsNonZero => !_isNull;

        /// <inheritdoc />
        public bool? IsPositive => _isNull.GetValueOrDefault();

        /// <inheritdoc />
        public bool? IsNegative => false;

        /// <inheritdoc />
        public IValue Copy() => new OValue(_isNull);
        
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