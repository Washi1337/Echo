using System;
using System.Collections;

namespace Echo.Concrete.Values.ValueType
{
    public abstract class PrimitiveNumberValue : ValueTypeValue
    {
        public abstract BitArray GetBits();
        
        public abstract BitArray GetMask();
        
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