using System;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents an object that is passed on by-value.
    /// </summary>
    public interface IValueTypeValue : IConcreteValue
    {
        /// <summary>
        /// Gets the raw bits of the primitive value.
        /// </summary>
        /// <param name="buffer">The buffer to write the raw bits to.</param>
        /// <remarks>
        /// The bits returned by this method assume the value is known entirely. Any bit that is marked unknown will be
        /// set to 0. 
        /// </remarks>
        public abstract void GetBits(Span<byte> buffer);

        /// <summary>
        /// Gets the bit mask indicating the bits that are known.  
        /// </summary>
        /// <param name="buffer">The buffer to write the raw mask to.</param>
        /// <remarks>
        /// If bit at location <c>i</c> equals 1, bit <c>i</c> is known, and unknown otherwise.
        /// </remarks>
        public abstract void GetMask(Span<byte> buffer);

        /// <summary>
        /// Replaces the raw contents of the integer with the provided bits and known mask.
        /// </summary>
        /// <param name="bits">The new bit values.</param>
        /// <param name="mask">The new bit mask indicating the known bits.</param>
        public abstract void SetBits(Span<byte> bits, Span<byte> mask);
    }
}