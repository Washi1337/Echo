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
            if (!is32Bit)
                throw new InvalidOperationException();
            return new NativeIntegerValue(this, true);
        }

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsU(bool is32Bit)
        {
            if (!is32Bit)
                throw new InvalidOperationException();
            return new NativeIntegerValue(this, true);
        }

        /// <inheritdoc />
        public I4Value InterpretAsI1()
        {
            uint signMask = GetBit(7).HasValue ? 0xFFFFFF00 : 0;
            int newValue = (I32 & 0xFF) | ~((I32 & 0x80) - 1);
            return new I4Value(newValue, (Mask & 0xFF) | signMask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU1() => new I4Value(I32 & 0xFF, (Mask & 0xFF) | 0xFFFFFF00);

        /// <inheritdoc />
        public I4Value InterpretAsI2()
        {
            uint signMask = GetBit(15).HasValue ? 0xFFFF0000 : 0;
            int newValue = (I32 & 0xFFFF) | ~((I32 & 0x8000) - 1);
            return new I4Value(newValue, (Mask & 0xFFFF) | signMask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU2() => new I4Value(I32 & 0xFFFF, (Mask & 0xFFFF) | 0xFFFF0000);

        /// <inheritdoc />
        public I4Value InterpretAsI4() => this;

        /// <inheritdoc />
        public I4Value InterpretAsU4() => this;

        /// <inheritdoc />
        public I8Value InterpretAsI8() => throw new InvalidOperationException();

        /// <inheritdoc />
        public unsafe FValue InterpretAsR4()
        {
            int bits = I32;
            return new FValue(*(float*) bits);
        }

        /// <inheritdoc />
        public FValue InterpretAsR8() => throw new InvalidOperationException();

        /// <inheritdoc />
        public OValue InterpretAsRef() => new OValue(IsZero);

        /// <inheritdoc />
        public NativeIntegerValue ConvertToI(bool is32Bit, bool unsigned, out bool overflowed)
        {
            overflowed = is32Bit && unsigned && U32 > int.MaxValue;
            return new NativeIntegerValue(this, is32Bit);
        }

        /// <inheritdoc />
        public NativeIntegerValue ConvertToU(bool is32Bit, bool unsigned, out bool overflowed)
        {
            overflowed = !unsigned && I32 < 0;
            return new NativeIntegerValue(this, is32Bit);
        }

        /// <inheritdoc />
        public I4Value ConvertToI1(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U32 > sbyte.MaxValue // || U32 < sbyte.MinValue;
                : I32 < sbyte.MinValue || I32 > sbyte.MaxValue;

            return InterpretAsI1();
        }

        /// <inheritdoc />
        public I4Value ConvertToU1(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U32 > byte.MaxValue // || U32 < byte.MinValue;
                : I32 < byte.MinValue || I32 > byte.MaxValue;

            return InterpretAsU1();
        }

        /// <inheritdoc />
        public I4Value ConvertToI2(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U32 > short.MaxValue // || U32 < short.MinValue;
                : I32 < short.MinValue || I32 > short.MaxValue;

            return InterpretAsI2();
        }

        /// <inheritdoc />
        public I4Value ConvertToU2(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned
                ? U32 > ushort.MaxValue // || U32 < ushort.MinValue;
                : I32 < ushort.MinValue || I32 > ushort.MaxValue;

            return InterpretAsU2();
        }

        /// <inheritdoc />
        public I4Value ConvertToI4(bool unsigned, out bool overflowed)
        {
            overflowed = unsigned && U32 > int.MaxValue; // || U32 < int.MinValue;
            return this;
        }

        /// <inheritdoc />
        public I4Value ConvertToU4(bool unsigned, out bool overflowed)
        {
            overflowed = !unsigned && I32 < uint.MinValue; // || I32 > uint.MaxValue;
            return this;
        }

        /// <inheritdoc />
        public I8Value ConvertToI8(bool unsigned, out bool overflowed)
        {
            overflowed = false;
            ulong signMask = GetLastBit().HasValue ? 0xFFFFFFFF00000000ul : 0ul;
            return new I8Value(I32, Mask | signMask);
        }

        /// <inheritdoc />
        public I8Value ConvertToU8(bool unsigned, out bool overflowed)
        {
            overflowed = !unsigned && I32 < 0;
            return new I8Value(U32, Mask | 0xFFFFFFFF00000000ul);
        }

        /// <inheritdoc />
        public FValue ConvertToR() => new FValue(U32);
    }
}
