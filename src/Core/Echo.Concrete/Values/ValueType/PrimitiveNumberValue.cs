using System;
using System.Collections;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a primitive numerical value that might contain unknown bits.
    /// </summary>
    public abstract class PrimitiveNumberValue : ValueTypeValue
    {
        /// <summary>
        /// Gets the raw bits of the primitive value.
        /// </summary>
        /// <returns>The raw bits.</returns>
        /// <remarks>
        /// The bits returned by this method assume the value is known entirely. Any bit that is marked unknown will be
        /// set to 0. 
        /// </remarks>
        public abstract BitArray GetBits();
        
        /// <summary>
        /// Gets the bit mask indicating the bits that are known.  
        /// </summary>
        /// <returns>
        /// The bit mask. If bit at location <c>i</c> equals 1, bit <c>i</c> is known, and unknown otherwise.
        /// </returns>
        public abstract BitArray GetMask();

        /// <inheritdoc />
        public override string ToString()
        {
            var bits = GetBits();
            var mask = GetMask();
            
            if (bits.Length != mask.Length)
                throw new ArgumentException("Bit lengths do not match.");

            var result = new char[bits.Length];
            
            for (int i = bits.Length - 1; i >= 0; i--)
            {
                result[i] = (bits[i] & mask[i]) 
                    ? '?' 
                    : bits[i] ? '1' : '0';
            }
            
            return new string(result);
        }   
        
    }
}