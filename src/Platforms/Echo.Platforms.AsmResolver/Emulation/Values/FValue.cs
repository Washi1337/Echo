using System;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
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
        public FValue InterpretAsR4()
        {
            double bits = F64;
            return new FValue(*(float*) &bits);
        }

        /// <inheritdoc />
        public FValue InterpretAsR8() => this;

        /// <inheritdoc />
        public OValue InterpretAsRef(bool is32Bit) => new OValue(null, is32Bit);

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
        public FValue ConvertToR() => this;

        /// <inheritdoc />
        public override IValue Copy() => new FValue(F64);
    }
}