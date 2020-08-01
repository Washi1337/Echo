using System;
using AsmResolver.DotNet.Memory;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    public partial class LleObjectValue : IPointerValue
    {
        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer) => Contents.ReadBytes(offset, memoryBuffer);

        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer, Span<byte> knownBitmaskBuffer) => 
            Contents.ReadBytes(offset, memoryBuffer, knownBitmaskBuffer);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data) => Contents.WriteBytes(offset, data);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data, ReadOnlySpan<byte> knownBitMask) => 
            Contents.WriteBytes(offset, data, knownBitMask);

        /// <inheritdoc />
        public Integer8Value ReadInteger8(int offset) => Contents.ReadInteger8(offset);

        /// <inheritdoc />
        public Integer16Value ReadInteger16(int offset) => Contents.ReadInteger16(offset);

        /// <inheritdoc />
        public Integer32Value ReadInteger32(int offset) => Contents.ReadInteger32(offset);

        /// <inheritdoc />
        public Integer64Value ReadInteger64(int offset) => Contents.ReadInteger64(offset);

        /// <inheritdoc />
        public Float32Value ReadFloat32(int offset) => Contents.ReadFloat32(offset);

        /// <inheritdoc />
        public Float64Value ReadFloat64(int offset) => Contents.ReadFloat64(offset);

        /// <inheritdoc />
        public void WriteInteger8(int offset, Integer8Value value) => Contents.WriteInteger8(offset, value);

        /// <inheritdoc />
        public void WriteInteger16(int offset, Integer16Value value) => Contents.WriteInteger16(offset, value);

        /// <inheritdoc />
        public void WriteInteger32(int offset, Integer32Value value) => Contents.WriteInteger32(offset, value);

        /// <inheritdoc />
        public void WriteInteger64(int offset, Integer64Value value) => Contents.WriteInteger64(offset, value);

        /// <inheritdoc />
        public void WriteFloat32(int offset, Float32Value value) => Contents.WriteFloat32(offset, value);

        /// <inheritdoc />
        public void WriteFloat64(int offset, Float64Value value) => Contents.WriteFloat64(offset, value);
    }
}