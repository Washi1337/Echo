using Echo.Core.Code;

namespace Echo.Platforms.JavaResolver
{
    /// <summary>
    ///     Represents an byteCode address range in memory.
    /// </summary>
    public readonly struct ByteCodeAddressRange : IAddressRange
    {
        /// <summary>
        ///     Determines whether two address ranges are considered equal.
        /// </summary>
        /// <param name="a">The first range.</param>
        /// <param name="b">The second range.</param>
        /// <returns><c>true</c> if the ranges are considered equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(ByteCodeAddressRange a, ByteCodeAddressRange b) => a.Equals(b);

        /// <summary>
        ///     Determines whether two address ranges are not considered equal.
        /// </summary>
        /// <param name="a">The first range.</param>
        /// <param name="b">The second range.</param>
        /// <returns><c>true</c> if the ranges are not considered equal, <c>false</c> otherwise.</returns>
        public static bool operator !=(ByteCodeAddressRange a, ByteCodeAddressRange b) => !a.Equals(b);

        /// <summary>
        ///     Creates a new byteCode address range.
        /// </summary>
        /// <param name="start">The starting address.</param>
        /// <param name="end">The exclusive ending address.</param>
        public ByteCodeAddressRange(long start, long end)
        {
            Start = start;
            End = end;
        }
        
        /// <inheritdoc />
        public long Start
        {
            get;
        }

        /// <inheritdoc />
        public long End
        {
            get;
        }

        /// <inheritdoc />
        public long Length => End - Start;

        /// <inheritdoc />
        public bool Contains(long address) => address >= Start && address < End;

        /// <inheritdoc />
        public bool Contains(IAddressRange range) => Contains(range.Start) && Contains(range.End);

        /// <inheritdoc />
        public bool Equals(in IAddressRange other) => 
            Start == other.Start && End == other.End;

        /// <inheritdoc />
        public override bool Equals(object obj) => 
            obj is ByteCodeAddressRange other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override string ToString() => 
            $"[{Start:X8}, {End:X8})";
    }
}