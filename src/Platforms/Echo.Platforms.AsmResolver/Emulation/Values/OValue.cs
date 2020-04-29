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
        /// <param name="is32Bit">Indicates whether the reference to the object is 32 or 64 bits wide.</param>
        public OValue(bool? isNull, bool is32Bit)
        {
            _isNull = isNull;
            Is32Bit = is32Bit;
        }
        
        /// <inheritdoc />
        public CliValueType CliValueType => CliValueType.O;

        /// <inheritdoc />
        public bool IsKnown => _isNull.HasValue;

        /// <summary>
        /// Gets a value indicating whether the reference to the object is 32 or 64 bits wide.
        /// </summary>
        public bool Is32Bit
        {
            get;
        }

        /// <inheritdoc />
        public int Size => Is32Bit ? 4 : 8;

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
        public virtual IValue Copy() => new OValue(_isNull, Is32Bit);

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsI(bool is32Bit)
        {
            var value = new NativeIntegerValue(0, is32Bit);
            if (!_isNull.GetValueOrDefault())
                value.MarkFullyUnknown();
            return value;
        }

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsU(bool is32Bit)
        {
            var value = new NativeIntegerValue(0, is32Bit);
            if (!_isNull.GetValueOrDefault())
                value.MarkFullyUnknown();
            return value;
        }

        /// <inheritdoc />
        public I4Value InterpretAsI1() => new I4Value(0, !_isNull.GetValueOrDefault() ? 0xFFFFFFFF : 0xFFFFFF00);

        /// <inheritdoc />
        public I4Value InterpretAsU1() => new I4Value(0, !_isNull.GetValueOrDefault() ? 0xFFFFFFFF : 0xFFFFFF00);

        /// <inheritdoc />
        public I4Value InterpretAsI2() => new I4Value(0, !_isNull.GetValueOrDefault() ? 0xFFFFFFFF : 0xFFFF0000);

        /// <inheritdoc />
        public I4Value InterpretAsU2() => new I4Value(0, !_isNull.GetValueOrDefault() ? 0xFFFFFFFF : 0xFFFF0000);

        /// <inheritdoc />
        public I4Value InterpretAsI4() => new I4Value(0, !_isNull.GetValueOrDefault() ? 0xFFFFFFFF : 0x00000000);

        /// <inheritdoc />
        public I4Value InterpretAsU4() => new I4Value(0, !_isNull.GetValueOrDefault() ? 0xFFFFFFFF : 0x00000000);

        /// <inheritdoc />
        public I8Value InterpretAsI8() => new I8Value(0, !_isNull.GetValueOrDefault() ? 0xFFFFFFFF_FFFFFFFF : 0);

        /// <inheritdoc />
        public FValue InterpretAsR4() => new FValue(0); // TODO: return unknown float.

        /// <inheritdoc />
        public FValue InterpretAsR8() => new FValue(0); // TODO: return unknown float.

        /// <inheritdoc />
        public OValue InterpretAsRef(bool is32Bit) => this;

        /// <inheritdoc />
        public NativeIntegerValue ConvertToI(bool is32Bit, bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return InterpretAsI(is32Bit);
        }

        /// <inheritdoc />
        public NativeIntegerValue ConvertToU(bool is32Bit, bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return InterpretAsU(is32Bit);
        }

        /// <inheritdoc />
        public I4Value ConvertToI1(bool unsigned, out bool overflowed) => throw new InvalidCastException();

        /// <inheritdoc />
        public I4Value ConvertToU1(bool unsigned, out bool overflowed) => throw new InvalidCastException();

        /// <inheritdoc />
        public I4Value ConvertToI2(bool unsigned, out bool overflowed) => throw new InvalidCastException();

        /// <inheritdoc />
        public I4Value ConvertToU2(bool unsigned, out bool overflowed) => throw new InvalidCastException();

        /// <inheritdoc />
        public I4Value ConvertToI4(bool unsigned, out bool overflowed) => throw new InvalidCastException();

        /// <inheritdoc />
        public I4Value ConvertToU4(bool unsigned, out bool overflowed) => throw new InvalidCastException();

        /// <inheritdoc />
        public I8Value ConvertToI8(bool unsigned, out bool overflowed) => throw new InvalidCastException();

        /// <inheritdoc />
        public I8Value ConvertToU8(bool unsigned, out bool overflowed) => throw new InvalidCastException();

        /// <inheritdoc />
        public FValue ConvertToR() => throw new InvalidCastException();
    }
}