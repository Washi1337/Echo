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
        /// <summary>
        /// Creates a new null pointer value.
        /// </summary>
        /// <param name="isKnown">Indicates whether the pointer is known.</param>
        public RelativePointerValue(bool isKnown)
        {
            IsKnown = isKnown;
        }
        
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
        /// <param name="offset">The offset relative to the base po[inter.</param>
        public RelativePointerValue(IPointerValue basePointer, int offset)
        {
            BasePointer = basePointer;
            CurrentOffset = offset;
            IsKnown = basePointer.IsKnown;
        }

        /// <summary>
        /// Gets the base memory pointer. 
        /// </summary>
        public IPointerValue BasePointer
        {
            get;
        }

        /// <summary>
        /// Gets or sets the current offset relative to the base pointer.
        /// </summary>
        public int CurrentOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the pointer is 32 bit or 64 bit wide.
        /// </summary>
        public bool Is32Bit => BasePointer.Is32Bit;

        /// <inheritdoc />
        public bool IsKnown
        {
            get;
        };

        /// <inheritdoc />
        /// <remarks>
        /// This property represents the size of the pointer, and not the size of the memory chunk that is referenced.
        /// </remarks>
        public int Size => Is32Bit ? 4 : 8;

        /// <inheritdoc />
        public bool IsValueType => false;

        /// <inheritdoc />
        public bool? IsZero => BasePointer is null;

        /// <inheritdoc />
        public bool? IsNonZero => !IsZero;

        /// <inheritdoc />
        public bool? IsPositive => true;

        /// <inheritdoc />
        public bool? IsNegative => false;

        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer) => 
            BasePointer.ReadBytes(CurrentOffset + offset, memoryBuffer);

        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer, Span<byte> knownBitmaskBuffer) => 
            BasePointer.ReadBytes(CurrentOffset + offset, memoryBuffer, knownBitmaskBuffer);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data) => 
            BasePointer.WriteBytes(CurrentOffset + offset, data);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data, ReadOnlySpan<byte> knownBitMask) => 
            BasePointer.WriteBytes(CurrentOffset + offset, data, knownBitMask);

        /// <inheritdoc />
        public Integer8Value ReadInteger8(int offset) => BasePointer.ReadInteger8(CurrentOffset + offset);

        /// <inheritdoc />
        public Integer16Value ReadInteger16(int offset) => BasePointer.ReadInteger16(CurrentOffset + offset);

        /// <inheritdoc />
        public Integer32Value ReadInteger32(int offset) => BasePointer.ReadInteger32(CurrentOffset + offset);

        /// <inheritdoc />
        public Integer64Value ReadInteger64(int offset) => BasePointer.ReadInteger64(CurrentOffset + offset);

        /// <inheritdoc />
        public Float32Value ReadFloat32(int offset) => BasePointer.ReadFloat32(CurrentOffset + offset);

        /// <inheritdoc />
        public Float64Value ReadFloat64(int offset) => BasePointer.ReadFloat64(CurrentOffset + offset);

        /// <inheritdoc />
        public void WriteInteger8(int offset, Integer8Value value) => 
            BasePointer.WriteInteger8(CurrentOffset + offset, value);

        /// <inheritdoc />
        public void WriteInteger16(int offset, Integer16Value value) => 
            BasePointer.WriteInteger16(CurrentOffset + offset, value);

        /// <inheritdoc />
        public void WriteInteger32(int offset, Integer32Value value) => 
            BasePointer.WriteInteger32(CurrentOffset + offset, value);

        /// <inheritdoc />
        public void WriteInteger64(int offset, Integer64Value value) => 
            BasePointer.WriteInteger64(CurrentOffset + offset, value);

        /// <inheritdoc />
        public void WriteFloat32(int offset, Float32Value value) => 
            BasePointer.WriteFloat32(CurrentOffset + offset, value);

        /// <inheritdoc />
        public void WriteFloat64(int offset, Float64Value value) => 
            BasePointer.WriteFloat64(CurrentOffset + offset, value);

        public void Add(int offset) => CurrentOffset += offset;

        public void Subtract(int offset) => CurrentOffset -= offset;

        /// <inheritdoc />
        public IValue Copy() => new RelativePointerValue(BasePointer, CurrentOffset);
    }
}