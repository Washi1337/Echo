using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Echo.Core;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a chunk of memory.
    /// </summary>
    public class MemoryBlockValue : IValueTypeValue, IMemoryAccessValue
    {
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Create();
        
        private readonly byte[] _memory;
        private readonly byte[] _knownBitMask;

        /// <summary>
        /// Creates a new uninitialized block of memory.
        /// </summary>
        /// <param name="size">The size of the memory.</param>
        public MemoryBlockValue(int size)
            : this(size, false)
        {
        }

        /// <summary>
        /// Creates a new block of memory.
        /// </summary>
        /// <param name="size">The size of the memory.</param>
        /// <param name="initialize">Determines whether the block should be initialized with zeroes or not.</param>
        public MemoryBlockValue(int size, bool initialize)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            
            _memory = Pool.Rent(size);
            _knownBitMask = Pool.Rent(size);
            Size = size;

            if (initialize)
            {
                _memory.AsSpan().Fill(0);
                _knownBitMask.AsSpan().Fill(0xFF);
            }
            else
            {
                _knownBitMask.AsSpan().Fill(0);
            }
        }

        /// <summary>
        /// Creates a new fully known memory block.
        /// </summary>
        /// <param name="memory">The referenced memory.</param>
        public MemoryBlockValue(Span<byte> memory)
            : this(memory.Length, false)
        {
            memory.CopyTo(_memory.AsSpan());
            _knownBitMask.AsSpan().Fill(0xFF);
        }

        /// <summary>
        /// Creates a new memory block.
        /// </summary>
        /// <param name="memory">The referenced memory.</param>
        /// <param name="knownBitMask">The bit mask indicating the known and unknown bits.</param>
        public MemoryBlockValue(Span<byte> memory, Span<byte> knownBitMask)
            : this(memory.Length, false)
        {
            SetBits(memory, knownBitMask);
        }

        ~MemoryBlockValue()
        {
            if (_memory is {})
                Pool.Return(_memory, true);

            if (_knownBitMask is {})
                Pool.Return(_knownBitMask, true);
        }
        
        /// <inheritdoc />
        public bool IsKnown
        {
            get
            {
                foreach (byte b in _knownBitMask)
                {
                    if (b != 0xFF)
                        return false;
                }

                return true;
            }
        }

        /// <inheritdoc />
        public int Size
        {
            get;
        }

        /// <inheritdoc />
        public bool IsValueType => true;

        /// <inheritdoc />
        public Trilean IsZero => false;

        /// <inheritdoc />
        public Trilean IsNonZero => true;

        /// <inheritdoc />
        public Trilean IsPositive => true;

        /// <inheritdoc />
        public Trilean IsNegative => false;

        /// <inheritdoc />
        public void GetBits(Span<byte> buffer) => 
            _memory.AsSpan(0, Math.Min(Size, buffer.Length)).CopyTo(buffer);

        /// <inheritdoc />
        public void GetMask(Span<byte> buffer) => 
            _knownBitMask.AsSpan(0, Math.Min(Size, buffer.Length)).CopyTo(buffer);

        /// <inheritdoc />
        public void SetBits(Span<byte> bits, Span<byte> mask)
        {
            bits.CopyTo(_memory.AsSpan(0, Math.Min(Size, bits.Length)));
            mask.CopyTo(_knownBitMask.AsSpan(0, Math.Min(Size, bits.Length)));
        }
        
        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer)
        {
            var slicedMemory = _memory.AsSpan(offset, Math.Min(Size - offset, memoryBuffer.Length));
            slicedMemory.CopyTo(memoryBuffer);
        }

        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer, Span<byte> knownBitmaskBuffer)
        {
            var slicedMemory = _memory.AsSpan(offset, Math.Min(Size - offset, memoryBuffer.Length));
            var slicedBitMask = _knownBitMask.AsSpan(offset, Math.Min(Size - offset, memoryBuffer.Length));

            slicedMemory.CopyTo(memoryBuffer);
            slicedBitMask.CopyTo(knownBitmaskBuffer);
        }
        
        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data)
        {
            var slicedMemory = _memory.AsSpan(offset, data.Length);
            var slicedBitMask = _knownBitMask.AsSpan(offset, data.Length);
            
            data.CopyTo(slicedMemory);
            slicedBitMask.Fill(0xFF);
        }

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data, ReadOnlySpan<byte> knownBitMask)
        {
            var slicedMemory = _memory.AsSpan(offset, data.Length);
            var slicedBitMask = _knownBitMask.AsSpan(offset, data.Length);
            
            data.CopyTo(slicedMemory);
            knownBitMask.CopyTo(slicedBitMask);
        }
        
        /// <inheritdoc />
        public Integer8Value ReadInteger8(int offset)
        {
            return OffsetIsInRange(offset, sizeof(byte))
                ? new Integer8Value(_memory[offset], _knownBitMask[offset])
                : new Integer8Value(0, 0);
        }

        /// <inheritdoc />
        public Integer16Value ReadInteger16(int offset)
        {
            if (!OffsetIsInRange(offset, sizeof(ushort)))
                return new Integer16Value(0, 0);
            
            ReadOnlySpan<byte> memorySpan = _memory.AsSpan(offset, 2);
            ReadOnlySpan<byte> knownBitsSpan = _knownBitMask.AsSpan(offset, 2);
            
            return new Integer16Value(
                BinaryPrimitives.ReadUInt16LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt16LittleEndian(knownBitsSpan));
        }

        /// <inheritdoc />
        public Integer32Value ReadInteger32(int offset)
        {
            if (!OffsetIsInRange(offset, sizeof(uint)))
                return new Integer32Value(0, 0);
            
            ReadOnlySpan<byte> memorySpan = _memory.AsSpan(offset, 4);
            ReadOnlySpan<byte> knownBitsSpan =  _knownBitMask.AsSpan(offset, 4);
            
            return new Integer32Value(
                BinaryPrimitives.ReadUInt32LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt32LittleEndian(knownBitsSpan));
        }

        /// <inheritdoc />
        public Integer64Value ReadInteger64(int offset)
        {
            if (!OffsetIsInRange(offset, sizeof(ulong)))
                return new Integer64Value(0, 0);
            
            ReadOnlySpan<byte> memorySpan = _memory.AsSpan(offset, 8);
            ReadOnlySpan<byte> knownBitsSpan =  _knownBitMask.AsSpan(offset, 8);
            
            return new Integer64Value(
                BinaryPrimitives.ReadUInt64LittleEndian(memorySpan), 
                BinaryPrimitives.ReadUInt64LittleEndian(knownBitsSpan));
        }

        /// <inheritdoc />
        public unsafe Float32Value ReadFloat32(int offset)
        {
            AssertOffsetValidity(offset, sizeof(float));
            
            // Note: There is unfortunately no BinaryPrimitives method for reading single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            fixed (byte* ptr = _memory)
            {
                return new Float32Value(*(float*) (ptr + offset));
            }
        }

        /// <inheritdoc />
        public unsafe Float64Value ReadFloat64(int offset)
        {
            AssertOffsetValidity(offset, sizeof(double));
            
            // Note: There is unfortunately no BinaryPrimitives method for reading single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            fixed (byte* ptr = _memory)
            {
                return new Float64Value(*(double*) (ptr + offset));
            }
        }

        /// <inheritdoc />
        public void WriteInteger8(int offset, Integer8Value value)
        {
            AssertOffsetValidity(offset, sizeof(byte));
            
            _memory[offset] = value.U8;
            _knownBitMask[offset] = value.Mask;
        }

        /// <inheritdoc />
        public void WriteInteger16(int offset, Integer16Value value)
        {
            AssertOffsetValidity(offset, sizeof(ushort));
            
            BinaryPrimitives.WriteUInt16LittleEndian(_memory.AsSpan(offset, 2), value.U16);
            BinaryPrimitives.WriteUInt16LittleEndian(_knownBitMask.AsSpan(offset, 2), value.Mask);
        }

        /// <inheritdoc />
        public void WriteInteger32(int offset, Integer32Value value)
        {
            AssertOffsetValidity(offset, sizeof(uint));
            
            BinaryPrimitives.WriteUInt32LittleEndian(_memory.AsSpan(offset, 4), value.U32);
            BinaryPrimitives.WriteUInt32LittleEndian(_knownBitMask.AsSpan(offset, 4), value.Mask);
        }

        /// <inheritdoc />
        public void WriteInteger64(int offset, Integer64Value value)
        {
            AssertOffsetValidity(offset, sizeof(ulong));
            
            BinaryPrimitives.WriteUInt64LittleEndian(_memory.AsSpan(offset, 8), value.U64);
            BinaryPrimitives.WriteUInt64LittleEndian(_knownBitMask.AsSpan(offset, 8), value.Mask);
        }

        /// <inheritdoc />
        public unsafe void WriteFloat32(int offset, Float32Value value)
        {
            AssertOffsetValidity(offset, sizeof(float));
            
            // Note: There is unfortunately no BinaryPrimitives method for writing single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            fixed (byte* ptr = _memory)
            {
                *(float*) (ptr + offset) = value.F32;
            }

            _knownBitMask.AsSpan(offset, 4).Fill(0xFF);
        }

        /// <inheritdoc />
        public unsafe void WriteFloat64(int offset, Float64Value value)
        {
            AssertOffsetValidity(offset, sizeof(double));
            
            // Note: There is unfortunately no BinaryPrimitives method for writing single or double values in 
            // .NET Standard 2.0. Hence we go the unsafe route.

            fixed (byte* ptr = _memory)
            {
                *(double*) (ptr + offset) = value.F64;
            }

            _knownBitMask.AsSpan(offset, 8).Fill(0xFF);
        }

        /// <inheritdoc />
        public IValue Copy() => new MemoryBlockValue(_memory, _knownBitMask);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AssertOffsetValidity(int offset, int size)
        {
            if (!OffsetIsInRange(offset, size))
                throw new ArgumentOutOfRangeException(nameof(offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool OffsetIsInRange(int offset, int size) => offset >= 0 && offset < _memory.Length - size + 1;

    }
}