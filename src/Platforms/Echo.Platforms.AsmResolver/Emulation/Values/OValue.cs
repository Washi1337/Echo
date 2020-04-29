using System;
using Echo.Concrete.Values;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents an object reference on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public class OValue : ICliValue
    {
        /// <summary>
        /// Creates a new null reference value.
        /// </summary>
        /// <param name="is32Bit">Indicates whether the reference to the object is 32 or 64 bits wide.</param>
        public OValue(bool is32Bit)
        {
            IsKnown = true;
            Is32Bit = is32Bit;
        }

        /// <summary>
        /// Creates a new object reference value.
        /// </summary>
        /// <param name="objectValue">The referenced value.</param>
        /// <param name="isKnown">Indicates whether the value is known.</param>
        /// <param name="is32Bit">Indicates whether the reference to the object is 32 or 64 bits wide.</param>
        public OValue(IConcreteValue objectValue, bool isKnown, bool is32Bit)
        {
            ObjectValue = objectValue;
            IsKnown = isKnown;
            Is32Bit = is32Bit;
        }

        /// <summary>
        /// Gets the object that was referenced.
        /// </summary>
        public IConcreteValue ObjectValue
        {
            get;
        }
        
        /// <inheritdoc />
        public CliValueType CliValueType => CliValueType.O;

        /// <inheritdoc />
        public bool IsKnown
        {
            get;
        }

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
        public bool? IsZero => IsKnown ? ObjectValue is null : (bool?) null;

        /// <inheritdoc />
        public bool? IsNonZero => !IsZero;

        /// <inheritdoc />
        public bool? IsPositive => !IsZero;

        /// <inheritdoc />
        public bool? IsNegative => false;

        /// <inheritdoc />
        public virtual IValue Copy() => new OValue(ObjectValue, IsKnown, Is32Bit);

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsI(bool is32Bit)
        {
            var value = new NativeIntegerValue(0, is32Bit);
            if (!IsZero.GetValueOrDefault())
                value.MarkFullyUnknown();
            return value;
        }

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsU(bool is32Bit)
        {
            var value = new NativeIntegerValue(0, is32Bit);
            if (!IsZero.GetValueOrDefault())
                value.MarkFullyUnknown();
            return value;
        }

        /// <inheritdoc />
        public I4Value InterpretAsI1() => new I4Value(0, IsZero.GetValueOrDefault() ? 0xFFFFFFFF : 0xFFFFFF00);

        /// <inheritdoc />
        public I4Value InterpretAsU1() => new I4Value(0, !IsZero.GetValueOrDefault() ? 0xFFFFFFFF : 0xFFFFFF00);

        /// <inheritdoc />
        public I4Value InterpretAsI2() => new I4Value(0, IsZero.GetValueOrDefault() ? 0xFFFFFFFF : 0xFFFF0000);

        /// <inheritdoc />
        public I4Value InterpretAsU2() => new I4Value(0, IsZero.GetValueOrDefault() ? 0xFFFFFFFF : 0xFFFF0000);

        /// <inheritdoc />
        public I4Value InterpretAsI4() => new I4Value(0, IsZero.GetValueOrDefault() ? 0xFFFFFFFF : 0x00000000);

        /// <inheritdoc />
        public I4Value InterpretAsU4() => new I4Value(0, IsZero.GetValueOrDefault() ? 0xFFFFFFFF : 0x00000000);

        /// <inheritdoc />
        public I8Value InterpretAsI8() => new I8Value(0, IsZero.GetValueOrDefault() ? 0xFFFFFFFF_FFFFFFFF : 0);

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