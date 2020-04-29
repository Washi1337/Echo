using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ReferenceType
{
    /// <summary>
    /// Represents a pointer to the beginning of a chunk of memory.
    /// </summary>
    public class MemoryPointerValue : IConcreteValue
    {
        private readonly Memory<byte> _memory;
        private readonly Memory<byte> _knownBitMask;
        private readonly bool _is32Bit;

        /// <summary>
        /// Creates a new memory pointer value.
        /// </summary>
        /// <param name="memory">The referenced memory.</param>
        /// <param name="knownBitMask">The bit mask indiating the known and unknown bits.</param>
        /// <param name="is32Bit">Indicates the pointer is 32 bit or 64 bit wide.</param>
        public MemoryPointerValue(Memory<byte> memory, Memory<byte> knownBitMask, bool is32Bit)
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

        /// <summary>
        /// Gets the length of the memory chunk that is referenced.
        /// </summary>
        public int Length => _memory.Length;

        /// <inheritdoc />
        /// <remarks>
        /// This property represents the size of the pointer, and not the size of the memory chunk that is referenced.
        /// </remarks>
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
        
        /// <summary>
        /// Reads a single 8 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public Integer8Value ReadInteger8(int offset)
        {
            AssertOffsetValidity(offset, sizeof(byte));
            return new Integer8Value(_memory.Span[offset], _knownBitMask.Span[offset]);
        }

        /// <summary>
        /// Reads a single 16 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public Integer16Value ReadInteger16(int offset)
        {
            AssertOffsetValidity(offset, sizeof(ushort));
            
            ReadOnlySpan<byte> memorySpan = _memory.Span.Slice(offset, 2);
            ReadOnlySpan<byte> knownBitsSpan = _knownBitMask.Span.Slice(offset, 2);
            
            return new Integer16Value(
                BinaryPrimitives.ReadUInt16LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt16LittleEndian(knownBitsSpan));
        }

        /// <summary>
        /// Reads a single 32 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public Integer32Value ReadInteger32(int offset)
        {
            AssertOffsetValidity(offset, sizeof(uint));
            
            ReadOnlySpan<byte> memorySpan = _memory.Span.Slice(offset, 4);
            ReadOnlySpan<byte> knownBitsSpan =  _knownBitMask.Span.Slice(offset, 4);
            
            return new Integer32Value(
                BinaryPrimitives.ReadUInt32LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt32LittleEndian(knownBitsSpan));
        }

        /// <summary>
        /// Reads a single 64 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public Integer64Value ReadInteger64(int offset)
        {
            AssertOffsetValidity(offset, sizeof(ulong));
            
            ReadOnlySpan<byte> memorySpan = _memory.Span.Slice(offset, 8);
            ReadOnlySpan<byte> knownBitsSpan =  _knownBitMask.Span.Slice(offset, 8);
            
            return new Integer64Value(
                BinaryPrimitives.ReadUInt64LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt64LittleEndian(knownBitsSpan));
        }

        /// <summary>
        /// Reads a single 32 bit floating point number at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read number.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public unsafe Float32Value ReadFloat32(int offset)
        {
            AssertOffsetValidity(offset, sizeof(float));
            
            // Note: There is unfortunately no BinaryPrimitives method for reading single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            using var handle = _memory.Pin();
            return new Float32Value(*(float*) ((byte*) handle.Pointer + offset));
        }

        /// <summary>
        /// Reads a single 64 bit floating point number at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read number.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public unsafe Float64Value ReadFloat64(int offset)
        {
            AssertOffsetValidity(offset, sizeof(double));
            
            // Note: There is unfortunately no BinaryPrimitives method for reading single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            using var handle = _memory.Pin();
            return new Float64Value(*(double*) ((byte*) handle.Pointer + offset));
        }

        /// <summary>
        /// Writes a single 8 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public void WriteInteger8(int offset, Integer8Value value)
        {
            AssertOffsetValidity(offset, sizeof(byte));
            
            _memory.Span[offset] = value.U8;
            _knownBitMask.Span[offset] = value.Mask;
        }

        /// <summary>
        /// Writes a single 16 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public void WriteInteger16(int offset, Integer16Value value)
        {
            AssertOffsetValidity(offset, sizeof(ushort));
            
            BinaryPrimitives.WriteUInt16LittleEndian(_memory.Span.Slice(offset, 2), value.U16);
            BinaryPrimitives.WriteUInt16LittleEndian(_knownBitMask.Span.Slice(offset, 2), value.Mask);
        }

        /// <summary>
        /// Writes a single 32 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public void WriteInteger32(int offset, Integer32Value value)
        {
            AssertOffsetValidity(offset, sizeof(uint));
            
            BinaryPrimitives.WriteUInt32LittleEndian(_memory.Span.Slice(offset, 4), value.U32);
            BinaryPrimitives.WriteUInt32LittleEndian(_knownBitMask.Span.Slice(offset, 4), value.Mask);
        }

        /// <summary>
        /// Writes a single 64 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public void WriteInteger64(int offset, Integer64Value value)
        {
            AssertOffsetValidity(offset, sizeof(ulong));
            
            BinaryPrimitives.WriteUInt64LittleEndian(_memory.Span.Slice(offset, 8), value.U64);
            BinaryPrimitives.WriteUInt64LittleEndian(_knownBitMask.Span.Slice(offset, 8), value.Mask);
        }

        /// <summary>
        /// Writes a single 32 bit floating point number at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public unsafe void WriteFloat32(int offset, Float32Value value)
        {
            AssertOffsetValidity(offset, sizeof(float));
            
            // Note: There is unfortunately no BinaryPrimitives method for writing single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            using var handle = _memory.Pin();
            *(float*) ((byte*) handle.Pointer + offset) = value.R4;
            _knownBitMask.Span.Slice(offset, 4).Fill(0xFF);
        }

        /// <summary>
        /// Writes a single 64 bit floating point number at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        public unsafe void WriteFloat64(int offset, Float64Value value)
        {
            AssertOffsetValidity(offset, sizeof(double));
            
            // Note: There is unfortunately no BinaryPrimitives method for writing single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            using var handle = _memory.Pin();
            *(double*) ((byte*) handle.Pointer + offset) = value.R8;
            _knownBitMask.Span.Slice(offset, 8).Fill(0xFF);
        }

        /// <inheritdoc />
        public IValue Copy() => new MemoryPointerValue(_memory, _knownBitMask, _is32Bit);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AssertOffsetValidity(int offset, int size)
        {
            if (offset < 0 || offset >= _memory.Length - size + 1)
                throw new ArgumentOutOfRangeException(nameof(offset));
        }
    }
}