namespace Echo.Core.Code
{
    /// <summary>
    /// Represents an address range in memory.
    /// </summary>
    public readonly struct AddressRange
    {
        /// <summary>
        /// Creates a new address range.
        /// </summary>
        /// <param name="start">The starting address.</param>
        /// <param name="end">The exclusive ending address.</param>
        public AddressRange(long start, long end)
        {
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
        /// <returns><c>true</c> if the address falls within the </returns>
        public bool Contains(long address) => address >= Start && address < End; 

        /// <inheritdoc />
        public override string ToString() => 
            $"[{Start:X8}, {End:X8})";
    }
}