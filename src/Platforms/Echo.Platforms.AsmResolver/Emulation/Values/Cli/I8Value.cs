using Echo.Concrete.Values;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Values.Cli
{
    /// <summary>
    /// Represents a 64 bit integer value on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public class I8Value : Integer64Value, ICliValue
    {
        /// <summary>
        /// Creates a new, fully known concrete 64 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        public I8Value(long value)
            : base(value)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 64 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public I8Value(long value, ulong mask)
            : base(value, mask)
        {
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 64 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        public I8Value(string bitString)
            : base(bitString)
        {
        }
        
        /// <inheritdoc />
        public CliValueType CliValueType => CliValueType.Int64;

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsI(bool is32Bit) => new NativeIntegerValue(this, is32Bit);

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsU(bool is32Bit) => new NativeIntegerValue(this, is32Bit);

        /// <inheritdoc />
        public I4Value InterpretAsI1()
        {
            uint signMask = GetBit(7).IsKnown ? 0xFFFFFF00 : 0;
            int newValue = (int) ((I64 & 0xFF) | ~((I64 & 0x80) - 1));
            return new I4Value(newValue, (uint) (Mask & 0xFF) | signMask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU1() => new I4Value((int) (I64 & 0xFF), (uint) (Mask & 0xFF) | 0xFFFFFF00);

        /// <inheritdoc />
        public I4Value InterpretAsI2()
        {
            uint signMask = GetBit(15).IsKnown ? 0xFFFF0000 : 0;
            int newValue = (int) ((I64 & 0xFFFF) | ~((I64 & 0x8000) - 1));
            return new I4Value(newValue, (uint) (Mask & 0xFFFF) | signMask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU2() => new I4Value((int) (I64 & 0xFFFF), (uint) (Mask & 0xFFFF) | 0xFFFF0000);

        /// <inheritdoc />
        public I4Value InterpretAsI4() => new I4Value( (int) (I64 & 0xFFFFFFFF), (uint) (Mask & 0xFFFFFFFF));

        /// <inheritdoc />
        public I4Value InterpretAsU4()  => new I4Value( (int) (U64 & 0xFFFFFFFF), (uint) (Mask & 0xFFFFFFFF));

        /// <inheritdoc />
        public I8Value InterpretAsI8() => this;

        /// <inheritdoc />
        public unsafe FValue InterpretAsR4()
        {
            long bits = I64;
            return new FValue(*(float*) bits);
        }

        /// <inheritdoc />
        public unsafe FValue InterpretAsR8() 
        {
            long bits = I64;
            return new FValue(*(double*) bits);
        }

        /// <inheritdoc />
        public OValue InterpretAsRef(bool is32Bit) => new OValue(null, IsZero.ToBooleanOrFalse(), true);

        /// <inheritdoc />
        public NativeIntegerValue ConvertToI(bool is32Bit, bool unsigned, out bool overflowed)
        {
            overflowed = is32Bit && unsigned && U64 > int.MaxValue;
            return new NativeIntegerValue(this, is32Bit);
        }

        /// <inheritdoc />
        public NativeIntegerValue ConvertToU(bool is32Bit, bool unsigned, out bool overflowed)
        {
            overflowed = !unsigned && I64 < 0;
            return new NativeIntegerValue(this, is32Bit);
        }

        /// <inheritdoc />
        public I4Value ConvertToI1(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U64 > (ulong) sbyte.MaxValue // || U64 < sbyte.MinValue;
                : I64 < sbyte.MinValue || I64 > sbyte.MaxValue;

            return InterpretAsI1();
        }

        /// <inheritdoc />
        public I4Value ConvertToU1(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U64 > byte.MaxValue // || U64 < byte.MinValue;
                : I64 < byte.MinValue || I64 > byte.MaxValue;

            return InterpretAsU1();
        }

        /// <inheritdoc />
        public I4Value ConvertToI2(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U64 > (ulong) short.MaxValue // || U64 < short.MinValue;
                : I64 < short.MinValue || I64 > short.MaxValue;

            return InterpretAsI2();
        }

        /// <inheritdoc />
        public I4Value ConvertToU2(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U64 > ushort.MaxValue // || U64 < ushort.MinValue;
                : I64 < ushort.MinValue || I64 > ushort.MaxValue;

            return InterpretAsU2();
        }

        /// <inheritdoc />
        public I4Value ConvertToI4(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U64 > int.MaxValue // || U64 < int.MinValue;
                : I64 < int.MinValue || I64 > int.MaxValue;
            
            return InterpretAsI4();
        }

        /// <inheritdoc />
        public I4Value ConvertToU4(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U64 > uint.MaxValue // || U64 < uint.MinValue;
                : I64 < uint.MinValue || I64 > uint.MaxValue;

            return InterpretAsU4();
        }

        /// <inheritdoc />
        public I8Value ConvertToI8(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned && U64 > long.MaxValue;
            return this;
        }

        /// <inheritdoc />
        public I8Value ConvertToU8(bool unsigned, out bool overflowed)
        {
            overflowed = !unsigned && I64 < 0;
            return this;
        }

        /// <inheritdoc />
        // ReSharper disable once RedundantCast
        public FValue ConvertToR4() => new FValue((float) I64);

        /// <inheritdoc />
        // ReSharper disable once RedundantCast
        public FValue ConvertToR8() => new FValue((double) I64);

        /// <inheritdoc />
        public FValue ConvertToR() => new FValue(U64);

        /// <inheritdoc />
        public override IValue Copy() => new I8Value(I64, Mask);
        
        /// <inheritdoc />
        public override string ToString() => $"int64 ({base.ToString()})";
    }
}