using System;
using System.Collections;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a native integer that is either 32-bits or 64-bits long, depending on the architecture the program
    /// is running on.
    /// </summary>
    public class NativeIntegerValue : IntegerValue, ICliValue
    {
        private readonly IntegerValue _value;

        /// <summary>
        /// Creates a fully known native integer value.
        /// </summary>
        /// <param name="value">The known integer value.</param>
        /// <param name="is32Bit">Indicates whether the integer should be resized to 32-bits or 64-bits.</param>
        public NativeIntegerValue(long value, bool is32Bit)
        {
            _value = is32Bit
                ? (IntegerValue) new Integer32Value((uint) (value & 0xFFFFFFFF))
                : new Integer64Value(value);
        }

        /// <summary>
        /// Parses a (partially) known bit string into an integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        /// <param name="is32Bit">Indicates whether the integer should be resized to 32-bits or 64-bits.</param>
        public NativeIntegerValue(string bitString, bool is32Bit)
            : this(new IntegerNValue(bitString), is32Bit)
        {
        }

        /// <summary>
        /// Converts the provided (partially) known integer value to a native integer.
        /// </summary>
        /// <param name="value">The partially known integer value.</param>
        /// <param name="is32Bit">Indicates whether the integer should be resized to 32-bits or 64-bits.</param>
        public NativeIntegerValue(IntegerValue value, bool is32Bit)
        {
            int newSize = is32Bit ? sizeof(uint) : sizeof(ulong);

            if (newSize < value.Size)
                _value = value.Truncate(newSize * 8);
            else if (newSize > value.Size)
                _value = value.Extend(newSize * 8, false);
            else
                _value = (IntegerValue) value.Copy();
        }

        /// <inheritdoc />
        public override bool IsKnown => _value.IsKnown;

        /// <inheritdoc />
        public override int Size => _value.Size;

        /// <inheritdoc />
        public override IValue Copy() => new NativeIntegerValue(_value, Size == sizeof(uint));

        /// <inheritdoc />
        public override bool? GetBit(int index) => _value.GetBit(index);

        /// <inheritdoc />
        public override void SetBit(int index, bool? value) => _value.SetBit(index, value);

        /// <inheritdoc />
        public override void GetBits(Span<byte> buffer) => _value.GetBits(buffer);

        /// <inheritdoc />
        public override void GetMask(Span<byte> buffer) => _value.GetMask(buffer);

        /// <inheritdoc />
        public override void SetBits(Span<byte> bits, Span<byte> mask) => _value.SetBits(bits, mask);

        /// <inheritdoc />
        public override void MarkFullyUnknown() => _value.MarkFullyUnknown();

        /// <summary>
        /// Converts the native integer value to a 64-bit integer.
        /// </summary>
        /// <returns></returns>
        public Integer64Value ToInt64()
        {
            return _value switch
            {
                Integer64Value int64 => int64,
                Integer32Value int32 => new Integer64Value(int32.I32, int32.Mask | 0xFFFFFFFF_00000000),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public NativeIntegerValue InterpretAsI(bool is32Bit)
        {
            return ((ICliValue) _value).InterpretAsI(is32Bit);
        }

        public NativeIntegerValue InterpretAsU(bool is32Bit)
        {
            return ((ICliValue) _value).InterpretAsU(is32Bit);
        }

        public I4Value InterpretAsI1()
        {
            return ((ICliValue) _value).InterpretAsI1();
        }

        public I4Value InterpretAsU1()
        {
            return ((ICliValue) _value).InterpretAsU1();
        }

        public I4Value InterpretAsI2()
        {
            return ((ICliValue) _value).InterpretAsI2();
        }

        public I4Value InterpretAsU2()
        {
            return ((ICliValue) _value).InterpretAsU2();
        }

        public I4Value InterpretAsI4()
        {
            return ((ICliValue) _value).InterpretAsI4();
        }

        public I4Value InterpretAsU4()
        {
            return ((ICliValue) _value).InterpretAsU4();
        }

        public I8Value InterpretAsI8()
        {
            return ((ICliValue) _value).InterpretAsI8();
        }

        public FValue InterpretAsR4()
        {
            return ((ICliValue) _value).InterpretAsR4();
        }

        public FValue InterpretAsR8()
        {
            return ((ICliValue) _value).InterpretAsR8();
        }

        public OValue InterpretAsRef()
        {
            return ((ICliValue) _value).InterpretAsRef();
        }

        public NativeIntegerValue ConvertToI(bool is32Bit, bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToI(is32Bit, unsigned, out overflowed);
        }

        public NativeIntegerValue ConvertToU(bool is32Bit, bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToU(is32Bit, unsigned, out overflowed);
        }

        public I4Value ConvertToI1(bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToI1(unsigned, out overflowed);
        }

        public I4Value ConvertToU1(bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToU1(unsigned, out overflowed);
        }

        public I4Value ConvertToI2(bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToI2(unsigned, out overflowed);
        }

        public I4Value ConvertToU2(bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToU2(unsigned, out overflowed);
        }

        public I4Value ConvertToI4(bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToI4(unsigned, out overflowed);
        }

        public I4Value ConvertToU4(bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToU4(unsigned, out overflowed);
        }

        public I8Value ConvertToI8(bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToI8(unsigned, out overflowed);
        }

        public I8Value ConvertToU8(bool unsigned, out bool overflowed)
        {
            return ((ICliValue) _value).ConvertToU8(unsigned, out overflowed);
        }

        public FValue ConvertToR()
        {
            return ((ICliValue) _value).ConvertToR();
        }
    }
}