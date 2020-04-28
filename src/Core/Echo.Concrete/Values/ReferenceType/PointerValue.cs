using System;
using System.Buffers.Binary;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ReferenceType
{
    public class PointerValue : IConcreteValue
    {
        private readonly Memory<byte> _memory;
        private readonly Memory<byte> _knownBitMask;
        private readonly bool _is32Bit;

        public PointerValue(Memory<byte> memory, Memory<byte> knownBitMask, bool is32Bit)
        {
            _memory = memory;
            _knownBitMask = knownBitMask;
            _is32Bit = is32Bit;
        }

        /// <inheritdoc />
        public bool IsKnown
        {
            get
            {
                foreach (byte b in _memory.Span)
                {
                    if (b != 0xFF)
                        return false;
                }

                return true;
            }
        }

        /// <inheritdoc />
        public int Size => _is32Bit ? 4 : 8;

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
        
        public Integer8Value ReadInteger8(int offset)
        {
            return new Integer8Value(_memory.Span[offset], _memory.Span[offset]);
        }

        public Integer16Value ReadInteger16(int offset)
        {
            ReadOnlySpan<byte> memorySpan = _memory.Span.Slice(offset, 2);
            ReadOnlySpan<byte> knownBitsSpan = _knownBitMask.Span.Slice(offset, 2);
            
            return new Integer16Value(
                BinaryPrimitives.ReadUInt16LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt16LittleEndian(knownBitsSpan));
        }

        public Integer32Value ReadInteger32(int offset)
        {
            ReadOnlySpan<byte> memorySpan = _memory.Span.Slice(offset, 4);
            ReadOnlySpan<byte> knownBitsSpan =  _knownBitMask.Span.Slice(offset, 4);
            
            return new Integer32Value(
                BinaryPrimitives.ReadUInt32LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt32LittleEndian(knownBitsSpan));
        }

        public Integer64Value ReadInteger64(int offset)
        {
            ReadOnlySpan<byte> memorySpan = _memory.Span.Slice(offset, 8);
            ReadOnlySpan<byte> knownBitsSpan =  _knownBitMask.Span.Slice(offset, 8);
            
            return new Integer64Value(
                BinaryPrimitives.ReadUInt64LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt64LittleEndian(knownBitsSpan));
        }

        public unsafe Float32Value ReadFloat32(int offset)
        {
            // Note: There is unfortunately no BinaryPrimitives method for reading single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            using var handle = _memory.Pin();
            return new Float32Value(*(float*) ((byte*) handle.Pointer + offset));
        }

        public unsafe Float64Value ReadFloat64(int offset)
        {
            // Note: There is unfortunately no BinaryPrimitives method for reading single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            using var handle = _memory.Pin();
            return new Float64Value(*(float*) ((byte*) handle.Pointer + offset));
        }

        public void WriteInteger8(int offset, Integer8Value value)
        {
            _memory.Span[offset] = value.U8;
            _knownBitMask.Span[offset] = value.Mask;
        }

        public void WriteInteger16(int offset, Integer16Value value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(_memory.Span.Slice(offset, 2), value.U16);
            BinaryPrimitives.WriteUInt16LittleEndian(_knownBitMask.Span.Slice(offset, 2), value.Mask);
        }


        public void WriteInteger32(int offset, Integer32Value value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(_memory.Span.Slice(offset, 4), value.U32);
            BinaryPrimitives.WriteUInt32LittleEndian(_knownBitMask.Span.Slice(offset, 4), value.Mask);
        }

        public void WriteInteger64(int offset, Integer64Value value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(_memory.Span.Slice(offset, 8), value.U64);
            BinaryPrimitives.WriteUInt64LittleEndian(_knownBitMask.Span.Slice(offset, 8), value.Mask);
        }

        public unsafe void WriteFloat32(int offset, Float32Value value)
        {
            // Note: There is unfortunately no BinaryPrimitives method for writing single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            using var handle = _memory.Pin();
            *(float*) ((byte*) handle.Pointer + offset) = value.R4;
            _knownBitMask.Span.Slice(offset, 4).Fill(0xFF);
        }

        public unsafe void WriteFloat64(int offset, Float64Value value)
        {
            // Note: There is unfortunately no BinaryPrimitives method for writing single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            using var handle = _memory.Pin();
            *(double*) ((byte*) handle.Pointer + offset) = value.R8;
            _knownBitMask.Span.Slice(offset, 8).Fill(0xFF);
        }

        /// <inheritdoc />
        public IValue Copy() => new PointerValue(_memory, _knownBitMask, _is32Bit);
    }
}