namespace Echo.Core.Code
{
    public interface IAddressRange
    {
        /// <summary>
        /// Gets the address of the first byte in the address range.
        /// </summary>
        public long Start { get; }

        /// <summary>
        /// Gets the address where this address range stops. This address is exclusive. 
        /// </summary>
        public long End { get; }

        /// <summary>
        /// Gets the total length of the address range.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Determines whether the provided address falls within the address range.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns><c>true</c> if the address falls within the range, <c>false otherwise</c>.</returns>
        public bool Contains(long address);

        /// <summary>
        /// Determines whether the address range contains the provided sub range.
        /// </summary>
        /// <param name="range">The address range.</param>
        /// <returns><c>true</c> if the sub range falls within the range, <c>false otherwise</c>.</returns>
        public bool Contains(IAddressRange range);

        /// <summary>
        /// Determines whether the range is considered equal with the provided range.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns><c>true</c> if the ranges are considered equal, <c>false</c> otherwise.</returns>
        public bool Equals(in IAddressRange other);
    }
}