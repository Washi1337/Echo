using System;
using Echo.Core.Code;

namespace Echo.Concrete.Memory
{
    /// <summary>
    /// Provides members for accessing and writing (partially known) memory.
    /// </summary>
    public interface IMemorySpace
    {
        /// <summary>
        /// Gets the range that this memory space spans.
        /// </summary>
        AddressRange AddressRange
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the provided address is a valid address, and can be used to read
        /// and/or write to.
        /// </summary>
        /// <param name="address">The address to query.</param>
        /// <returns><c>true</c> if the address was valid, <c>false</c> otherwise.</returns>
        bool IsValidAddress(long address);

        /// <summary>
        /// Relocates the memory to a new base address.
        /// </summary>
        /// <param name="baseAddress">The new base address.</param>
        void Rebase(long baseAddress);
        
        /// <summary>
        /// Copies data at the provided address into the provided buffer. 
        /// </summary>
        /// <param name="address">The address to start reading at.</param>
        /// <param name="buffer">The buffer to write into.</param>
        void Read(long address, BitVectorSpan buffer);
        
        /// <summary>
        /// Writes the provided buffer of data at the provided address. 
        /// </summary>
        /// <param name="address">The address to start writing at.</param>
        /// <param name="buffer">The data to write.</param>
        void Write(long address, BitVectorSpan buffer);
        
        /// <summary>
        /// Writes the provided buffer of data at the provided address. 
        /// </summary>
        /// <param name="address">The address to start writing at.</param>
        /// <param name="buffer">The data to write.</param>
        void Write(long address, ReadOnlySpan<byte> buffer);
    }
}