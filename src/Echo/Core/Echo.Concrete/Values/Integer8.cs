namespace Echo.Concrete.Values
{
    /// <summary>
    /// Represents a (partially) known concrete 8 bit integral value.
    /// </summary>
    public class Integer8 : ValueTypeValue
    {
        public static implicit operator Integer8(byte value)
        {
            return new Integer8(value);
        }

        public static implicit operator Integer8(sbyte value)
        {
            return new Integer8(value);
        }
        
        public const byte FullyKnownMask = 0xFF;
        
        private byte _value;
        
        public Integer8(byte value)
            : this(value, 0xFF)
        {
        }

        public Integer8(sbyte value)
            : this(value, 0xFF)
        {
        }

        public Integer8(sbyte value, byte mask)
            : this(unchecked((byte) value), mask)
        {
        }

        public Integer8(byte value, byte mask)
        {
            _value = value;
            Mask = mask;
        }

        /// <inheritdoc />
        public override bool IsKnown => Mask == FullyKnownMask;

        /// <inheritdoc />
        public override int Size => sizeof(byte);

        /// <summary>
        /// Gets the signed representation of this 8 bit value.
        /// </summary>
        public sbyte I8
        {
            get => unchecked((sbyte) U8);
            set => U8 = unchecked((byte) value);
        }

        /// <summary>
        /// Gets the unsigned representation of this 8 bit value.
        /// </summary>
        public byte U8
        {
            get => (byte) (_value & Mask);
            set => _value = value;
        }

        /// <summary>
        /// Gets a value indicating which bits in the integer are known.
        /// If bit at location <c>i</c> equals 1, bit <c>i</c> in <see cref="I8"/> and <see cref="U8"/> is known,
        /// and unknown otherwise.  
        /// </summary>
        public byte Mask
        {
            get;
            set;
        }

        public override string ToString()
        {
            var bits = new char[Size * 8];

            for (int i = 0, j = 1 << (bits.Length - 1); i < bits.Length; i++, j >>= 1)
            {
                bits[i] = (Mask & j) == 0 
                    ? '?' 
                    : (_value & j) != 0 ? '1' : '0';
            }
            
            return new string(bits);
        }
    }
}