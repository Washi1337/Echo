using System;
using System.Globalization;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Values.Cli
{
    /// <summary>
    /// Represents a floating point numerical value on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public unsafe class FValue : Float64Value, ICliValue
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
        public CliValueType CliValueType => CliValueType.F;

        /// <summary>
        /// Determines whether the floating point value is considered greater than the provided floating point value.
        /// </summary>
        /// <param name="other">The other floating point value.</param>
        /// <param name="allowUnordered">Determines the return value when one of the values is NaN.</param>
        /// <returns><c>true</c> if the current value is greater than the provided value, <c>false</c> otherwise.</returns>
        public bool IsGreaterThan(FValue other, bool allowUnordered)
        {
            // C# compiler emits cgt for the ">" operator with floating point operands.
            return double.IsNaN(F64) || double.IsNaN(other.F64)
                ? allowUnordered
                : F64 > other.F64;
        }

        /// <summary>
        /// Determines whether the floating point value is considered less than the provided floating point value.
        /// </summary>
        /// <param name="other">The other floating point value.</param>
        /// <param name="allowUnordered">Determines the return value when one of the values is NaN.</param>
        /// <returns><c>true</c> if the current value is less than the provided value, <c>false</c> otherwise.</returns>
        public bool IsLessThan(FValue other, bool allowUnordered)
        {
            // C# compiler emits clt for the "<" operator with floating point operands.
            return double.IsNaN(F64) || double.IsNaN(other.F64)
                ? allowUnordered
                : F64 < other.F64;
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
            double bits = F64;
            return new I4Value(*(sbyte*) &bits);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU1()
        {
            double bits = F64;
            return new I4Value(*(byte*) &bits);
        }

        /// <inheritdoc />
        public I4Value InterpretAsI2()
        {
            double bits = F64;
            return new I4Value(*(short*) &bits);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU2()
        {
            double bits = F64;
            return new I4Value(*(ushort*) &bits);
        }

        /// <inheritdoc />
        public I4Value InterpretAsI4()
        {
            double bits = F64;
            return new I4Value(*(int*) &bits);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU4()
        {
            double bits = F64;
            return new I4Value(*(int*) &bits);
        }

        /// <inheritdoc />
        public I8Value InterpretAsI8()
        {
            double bits = F64;
            return new I8Value(*(long*) &bits);
        }

        /// <inheritdoc />
        public FValue InterpretAsR4() => new FValue((float) F64);

        /// <inheritdoc />
        public FValue InterpretAsR8() => this;

        /// <inheritdoc />
        public OValue InterpretAsRef(bool is32Bit) => new OValue(null, true, is32Bit);

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
            overflowed = false;
            return new I4Value(unchecked((sbyte) (int) F64));
        }

        /// <inheritdoc />
        public I4Value ConvertToU1(bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return new I4Value(unchecked((byte) (int) F64));
        }

        /// <inheritdoc />
        public I4Value ConvertToI2(bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return new I4Value(unchecked((short) (int) F64));
        }

        /// <inheritdoc />
        public I4Value ConvertToU2(bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return new I4Value(unchecked((ushort) (int) F64));
        }

        /// <inheritdoc />
        public I4Value ConvertToI4(bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return new I4Value(unchecked((int) F64));
        }

        /// <inheritdoc />
        public I4Value ConvertToU4(bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return new I4Value(unchecked((int) F64));
        }

        /// <inheritdoc />
        public I8Value ConvertToI8(bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return new I8Value(unchecked((long) F64));
        }

        /// <inheritdoc />
        public I8Value ConvertToU8(bool unsigned, out bool overflowed)
        {
            overflowed = false;
            return new I8Value(unchecked((long) F64));
        }

        /// <inheritdoc />
        public FValue ConvertToR4() => new FValue((float) F64);

        /// <inheritdoc />
        public FValue ConvertToR8() => this;
        
        /// <inheritdoc />
        public FValue ConvertToR() => this;

        /// <inheritdoc />
        public override IValue Copy() => new FValue(F64);

        /// <inheritdoc />
        public override string ToString() => $"F ({F64.ToString(CultureInfo.InvariantCulture)})";
    }
}