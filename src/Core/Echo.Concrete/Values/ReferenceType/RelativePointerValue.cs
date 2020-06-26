using System;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ReferenceType
{
    /// <summary>
    /// Represents a pointer that is relative to a base pointer. 
    /// </summary>
    public class RelativePointerValue : IPointerValue
    {
        private readonly IPointerValue _basePointer;
        private readonly int _baseOffset;

        /// <summary>
        /// Creates a new relative pointer value.
        /// </summary>
        /// <param name="basePointer">The base memory pointer.</param>
        public RelativePointerValue(IPointerValue basePointer)
            : this(basePointer, 0)
        {
        }

        /// <summary>
        /// Creates a new relative pointer value.
        /// </summary>
        /// <param name="basePointer">The base memory pointer.</param>
        /// <param name="baseOffset">The base offset.</param>
        public RelativePointerValue(IPointerValue basePointer, int baseOffset)
        {
            _basePointer = basePointer ?? throw new ArgumentNullException(nameof(basePointer));
            _baseOffset = baseOffset;
        }

        /// <summary>
        /// Gets a value indicating whether the pointer is 32 bit or 64 bit wide.
        /// </summary>
        public bool Is32Bit => _basePointer.Is32Bit;

        /// <inheritdoc />
        public bool IsKnown => true;

        /// <inheritdoc />
        /// <remarks>
        /// This property represents the size of the pointer, and not the size of the memory chunk that is referenced.
        /// </remarks>
        public int Size => Is32Bit ? 4 : 8;

        /// <inheritdoc />
        public bool IsValueType => false;

        /// <inheritdoc />
        public bool? IsZero => false;

        /// <inheritdoc />
        public bool? IsNonZero => true;

        /// <inheritdoc />
        public bool? IsPositive => true;

        /// <inheritdoc />
        public bool? IsNegative => false;

        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer) => 
            _basePointer.ReadBytes(_baseOffset + offset, memoryBuffer);

        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer, Span<byte> knownBitmaskBuffer) => 
            _basePointer.ReadBytes(_baseOffset + offset, memoryBuffer, knownBitmaskBuffer);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data) => 
            _basePointer.WriteBytes(_baseOffset + offset, data);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data, ReadOnlySpan<byte> knownBitMask) => 
            _basePointer.WriteBytes(_baseOffset + offset, data, knownBitMask);

        /// <inheritdoc />
        public Integer8Value ReadInteger8(int offset) => _basePointer.ReadInteger8(_baseOffset + offset);

        /// <inheritdoc />
        public Integer16Value ReadInteger16(int offset) => _basePointer.ReadInteger16(_baseOffset + offset);

        /// <inheritdoc />
        public Integer32Value ReadInteger32(int offset) => _basePointer.ReadInteger32(_baseOffset + offset);

        /// <inheritdoc />
        public Integer64Value ReadInteger64(int offset) => _basePointer.ReadInteger64(_baseOffset + offset);

        /// <inheritdoc />
        public Float32Value ReadFloat32(int offset) => _basePointer.ReadFloat32(_baseOffset + offset);

        /// <inheritdoc />
        public Float64Value ReadFloat64(int offset) => _basePointer.ReadFloat64(_baseOffset + offset);

        /// <inheritdoc />
        public void WriteInteger8(int offset, Integer8Value value) => 
            _basePointer.WriteInteger8(_baseOffset + offset, value);

        /// <inheritdoc />
        public void WriteInteger16(int offset, Integer16Value value) => 
            _basePointer.WriteInteger16(_baseOffset + offset, value);

        /// <inheritdoc />
        public void WriteInteger32(int offset, Integer32Value value) => 
            _basePointer.WriteInteger32(_baseOffset + offset, value);

        /// <inheritdoc />
        public void WriteInteger64(int offset, Integer64Value value) => 
            _basePointer.WriteInteger64(_baseOffset + offset, value);

        /// <inheritdoc />
        public void WriteFloat32(int offset, Float32Value value) => 
            _basePointer.WriteFloat32(_baseOffset + offset, value);

        /// <inheritdoc />
        public void WriteFloat64(int offset, Float64Value value) => 
            _basePointer.WriteFloat64(_baseOffset + offset, value);

        /// <inheritdoc />
        public IPointerValue Add(int offset) =>
            new RelativePointerValue(_basePointer, _baseOffset + offset);

        /// <inheritdoc />
        public IPointerValue Subtract(int offset) => 
            new RelativePointerValue(_basePointer, _baseOffset - offset);

        /// <inheritdoc />
        public IValue Copy() => new RelativePointerValue(_basePointer, _baseOffset);
    }
}