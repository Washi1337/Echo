using System;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;

namespace Echo.Concrete.Values
{
    /// <summary>
    /// Provides memory read/write operations on a concrete value.
    /// </summary>
    public interface IMemoryAccessValue : IConcreteValue
    {
        /// <summary>
        /// Reads raw contents of the memory block. 
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <param name="memoryBuffer">The memory buffer to copy data to.</param>
        /// <remarks>
        /// This method has undefined behaviour if the memory contains unknown bits.
        /// </remarks>
        void ReadBytes(int offset, Span<byte> memoryBuffer);

        /// <summary>
        /// Reads raw contents of the memory block. 
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <param name="memoryBuffer">The memory buffer to copy data to.</param>
        /// <param name="knownBitmaskBuffer">The buffer to copy the known bitmask to.</param>
        void ReadBytes(int offset, Span<byte> memoryBuffer, Span<byte> knownBitmaskBuffer);

        /// <summary>
        /// Writes raw data to the memory block as fully known bytes.
        /// </summary>
        /// <param name="offset">The offset to start writing.</param>
        /// <param name="data">The data to write.</param>
        void WriteBytes(int offset, ReadOnlySpan<byte> data);

        /// <summary>
        /// Writes raw data to the memory block.
        /// </summary>
        /// <param name="offset">The offset to start writing.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="knownBitMask">
        /// The bitmask indicating the bits that are known within the data referenced by <paramref name="data"/>.
        /// </param>
        void WriteBytes(int offset, ReadOnlySpan<byte> data, ReadOnlySpan<byte> knownBitMask);

        /// <summary>
        /// Reads a single 8 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        Integer8Value ReadInteger8(int offset);

        /// <summary>
        /// Reads a single 16 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        Integer16Value ReadInteger16(int offset);

        /// <summary>
        /// Reads a single 32 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        Integer32Value ReadInteger32(int offset);

        /// <summary>
        /// Reads a single 64 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        Integer64Value ReadInteger64(int offset);

        /// <summary>
        /// Reads a single 32 bit floating point number at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read number.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        Float32Value ReadFloat32(int offset);

        /// <summary>
        /// Reads a single 64 bit floating point number at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <returns>The read number.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        Float64Value ReadFloat64(int offset);

        /// <summary>
        /// Writes a single 8 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        void WriteInteger8(int offset, Integer8Value value);

        /// <summary>
        /// Writes a single 16 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        void WriteInteger16(int offset, Integer16Value value);

        /// <summary>
        /// Writes a single 32 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        void WriteInteger32(int offset, Integer32Value value);

        /// <summary>
        /// Writes a single 64 bit integer at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        void WriteInteger64(int offset, Integer64Value value);

        /// <summary>
        /// Writes a single 32 bit floating point number at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        void WriteFloat32(int offset, Float32Value value);

        /// <summary>
        /// Writes a single 64 bit floating point number at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        void WriteFloat64(int offset, Float64Value value);
    }

    /// <summary>
    /// Provides extension methods for the <see cref="IMemoryAccessValue"/> interface.
    /// </summary>
    public static class MemoryAccessValueExtensions
    {
        /// <summary>
        /// Creates a new pointer to the memory.
        /// </summary>
        /// <param name="self">The memory to reference.</param>
        /// <param name="is32Bit">Indicates whether the pointer is 32 or 64 bits wide.</param>
        /// <returns>The constructed pointer.</returns>
        public static IPointerValue MakePointer(this IMemoryAccessValue self, bool is32Bit) => 
            new RelativePointerValue(self, is32Bit);

        /// <summary>
        /// Creates a new pointer to an offset within the memory.
        /// </summary>
        /// <param name="self">The memory to reference.</param>
        /// <param name="offset">The offset within the memory.</param>
        /// <param name="is32Bit">Indicates whether the pointer is 32 or 64 bits wide.</param>
        /// <returns>The constructed pointer.</returns>
        public static IPointerValue MakePointer(this IMemoryAccessValue self, int offset, bool is32Bit) => 
            new RelativePointerValue(self, offset, is32Bit);
    }
}