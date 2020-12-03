using System;

namespace Echo.Core.Code
{
    /// <summary>
    /// Represents an address range in memory.
    /// </summary>
    public readonly struct AddressRange
    {
        /// <summary>
        /// A range that starts and ends at index 0.
        /// </summary>
        public static readonly AddressRange NilRange = new AddressRange(0, 0);
        
        /// <summary>
        /// Determines whether two address ranges are considered equal.
        /// </summary>
        /// <param name="a">The first range.</param>
        /// <param name="b">The second range.</param>
        /// <returns><c>true</c> if the ranges are considered equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(AddressRange a, AddressRange b) => a.Equals(b);

        /// <summary>
        /// Determines whether two address ranges are not considered equal.
        /// </summary>
        /// <param name="a">The first range.</param>
        /// <param name="b">The second range.</param>
        /// <returns><c>true</c> if the ranges are not considered equal, <c>false</c> otherwise.</returns>
        public static bool operator !=(AddressRange a, AddressRange b) => !a.Equals(b);

        /// <summary>
        /// Creates a new address range.
        /// </summary>
        /// <param name="start">The starting address.</param>
        /// <param name="end">The exclusive ending address.</param>
        public AddressRange(long start, long end)
        {
            if (end < start)
                throw new ArgumentOutOfRangeException(nameof(end), "End address is lower than start address.");
            
            Start = start;
            End = end;
        }
        
        /// <summary>
        /// Gets the address of the first byte in the address range.
        /// </summary>
        public long Start
        {
            get;
        }

        /// <summary>
        /// Gets the address where this address range stops. This address is exclusive. 
        /// </summary>
        public long End
        {
            get;
        }

        /// <summary>
        /// Gets the total length of the address range.
        /// </summary>
        public long Length => End - Start;

        /// <summary>
        /// Determines whether the provided address falls within the address range.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns><c>true</c> if the address falls within the range, <c>false otherwise</c>.</returns>
        public bool Contains(long address) => address >= Start && address < End;

        /// <summary>
        /// Determines whether the address range contains the provided sub range.
        /// </summary>
        /// <param name="range">The address range.</param>
        /// <returns><c>true</c> if the sub range falls within the range, <c>false otherwise</c>.</returns>
        public bool Contains(AddressRange range) => Contains(range.Start) && Contains(range.End);

        /// <summary>
        /// Determines whether the range is considered equal with the provided range.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns><c>true</c> if the ranges are considered equal, <c>false</c> otherwise.</returns>
        public bool Equals(in AddressRange other) => 
            Start == other.Start && End == other.End;

        /// <inheritdoc />
        public override bool Equals(object obj) => 
            obj is AddressRange other && Equals(other);

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