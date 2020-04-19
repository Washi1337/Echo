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
    public class NativeIntegerValue : IntegerValue
    {
        private readonly IntegerValue _value;

        /// <summary>
        /// Creates a fully known native integer value.
        /// </summary>
        /// <param name="value">The known integer value.</param>
        /// <param name="is32Bit">Indicates whether the integer should be resized to 32-bits or 64-bits.</param>
        public NativeIntegerValue(IntPtr value, bool is32Bit)
        {
            _value = is32Bit
                ? (IntegerValue) new Integer32Value(value.ToInt32())
                : new Integer64Value(value.ToInt64());
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
                _value = value.Truncate(newSize);
            else if (newSize > value.Size)
                _value = value.Extend(newSize, false);
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
        public override BitArray GetBits() => _value.GetBits();

        /// <inheritdoc />
        public override BitArray GetMask() => _value.GetMask();

        /// <inheritdoc />
        public override void SetBits(BitArray bits, BitArray mask) => _value.SetBits(bits, mask);
    }
}