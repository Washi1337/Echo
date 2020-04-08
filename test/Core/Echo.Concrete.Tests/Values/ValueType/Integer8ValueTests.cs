using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Integer8Tests
    {
        [Fact]
        public void KnownValueSignedUnsignedSame()
        {
            var value = new Integer8Value(0x12);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer8Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x12, value.U8);
            Assert.Equal(0x12, value.I8);
        }
        
        [Fact]
        public void KnownValueSignedUnsignedDifferent()
        {
            var value = new Integer8Value(0x80);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer8Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x80, value.U8);
            Assert.Equal(-0x80, value.I8);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer8Value(
                0b00110011, 
                0b11101101);
            
            Assert.False(value.IsKnown);
            Assert.Equal(0b00100001, value.U8);
        }

        [Fact]
        public void LittleEndianBitOrder()
        {
            var value = new Integer8Value(0b0001_0010);

            var bits = new bool[8];
            value.GetBits().CopyTo(bits, 0);
            Assert.Equal(new[]
            {
                false, true, false, false, true, false, false, false
            }, bits);
        }

        [Fact]
        public void AddFullyKnownValues()
        {
            var value1 = new Integer8Value(0b0001_0010);
            var value2 = new Integer8Value(0b0011_0100);

            value1.Add(value2);
            
            Assert.Equal(0b0100_0110, value1.U8);
        }

        [Fact]
        public void AddKnownBitToUnknownBitShouldResultInUnknownCarry()
        {
            var value1 = new Integer8Value(
                0b0000_0000,
                0b1111_1110);
            var value2 = new Integer8Value(0b0000_0001);

            value1.Add(value2);
            
            Assert.Equal(0b1111_1100, value1.Mask);
        }

        [Fact]
        public void AddUnknownBitToUnknownBitShouldResultInUnknownCarry()
        {
            var value1 = new Integer8Value(
                0b0000_0000,
                0b1111_1110);
            var value2 = new Integer8Value(0b0000_0000, 0b1111_1110);

            value1.Add(value2);
            
            Assert.Equal(0b1111_1100, value1.Mask);
        }
        
        [Fact]
        public void AddKnownBitToUnknownBitsShouldResultInUnknownRippleCarry()
        {
            var value1 = new Integer8Value(
                0b0000_0000,
                0b1111_1100);
            var value2 = new Integer8Value(0b0000_0001);

            value1.Add(value2);
            
            Assert.Equal(0b1111_1000, value1.Mask);
        }
        
        [Fact]
        public void AddKnownBitToKnownBitsThatCarryOverToUnknownBitsShouldRippleCarry()
        {
            var value1 = new Integer8Value(
                0b0000_0011,
                0b1111_0011);
            var value2 = new Integer8Value(0b0000_0001);

            value1.Add(value2);
            
            Assert.Equal(0b1110_0011, value1.Mask);
        }
        
        [Fact]
        public void AddUnknownBitToKnownBitsThatCarryOverToUnknownBitsShouldRippleCarry()
        {
            var value1 = new Integer8Value(
                0b0000_0011,
                0b1111_0011);
            var value2 = new Integer8Value(
                0b0000_0000, 
                0b1111_1110);

            value1.Add(value2);
            
            Assert.Equal(0b1110_0000, value1.Mask);
        }
    }
}