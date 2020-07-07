using System;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    public partial class LleObjectValue : IPointerValue
    {
        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer) => _contents.ReadBytes(offset, memoryBuffer);

        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer, Span<byte> knownBitmaskBuffer) => 
            _contents.ReadBytes(offset, memoryBuffer, knownBitmaskBuffer);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data) => _contents.WriteBytes(offset, data);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data, ReadOnlySpan<byte> knownBitMask) => 
            _contents.WriteBytes(offset, data, knownBitMask);

        /// <inheritdoc />
        public Integer8Value ReadInteger8(int offset) => _contents.ReadInteger8(offset);

        /// <inheritdoc />
        public Integer16Value ReadInteger16(int offset) => _contents.ReadInteger16(offset);

        /// <inheritdoc />
        public Integer32Value ReadInteger32(int offset) => _contents.ReadInteger32(offset);

        /// <inheritdoc />
        public Integer64Value ReadInteger64(int offset) => _contents.ReadInteger64(offset);

        /// <inheritdoc />
        public Float32Value ReadFloat32(int offset) => _contents.ReadFloat32(offset);

        /// <inheritdoc />
        public Float64Value ReadFloat64(int offset) => _contents.ReadFloat64(offset);

        /// <inheritdoc />
        public void WriteInteger8(int offset, Integer8Value value) => _contents.WriteInteger8(offset, value);

        /// <inheritdoc />
        public void WriteInteger16(int offset, Integer16Value value) => _contents.WriteInteger16(offset, value);

        /// <inheritdoc />
        public void WriteInteger32(int offset, Integer32Value value) => _contents.WriteInteger32(offset, value);

        /// <inheritdoc />
        public void WriteInteger64(int offset, Integer64Value value) => _contents.WriteInteger64(offset, value);

        /// <inheritdoc />
        public void WriteFloat32(int offset, Float32Value value) => _contents.WriteFloat32(offset, value);

        /// <inheritdoc />
        public void WriteFloat64(int offset, Float64Value value) => _contents.WriteFloat64(offset, value);
    }
}