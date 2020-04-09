using System;
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
        public void ParseFullyKnownBitString()
        {
            var value = new Integer8Value("00001100");
            Assert.Equal(0b00001100, value.U8);
        }

        [Fact]
        public void ParsePartiallyKnownBitString()
        {
            var value = new Integer8Value("000011??");
            Assert.Equal(0b11111100, value.Mask);
            Assert.Equal(0b00001100, value.U8 & value.Mask);
        }

        [Fact]
        public void ParseFewerBits()
        {
            var value = new Integer8Value("101");
            Assert.Equal(0b101, value.U8);
        }

        [Fact]
        public void ParseWithMoreZeroes()
        {
            var value = new Integer8Value("0000000000000101");
            Assert.Equal(0b101, value.U8);
        }

        [Fact]
        public void ParseWithOverflow()
        {
            Assert.Throws<OverflowException>(() => new Integer8Value("10000000000000101"));
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
            var value1 = new Integer8Value("0000000?");
            var value2 = new Integer8Value("00000001");

            value1.Add(value2);
            
            Assert.Equal(0b1111_1100, value1.Mask);
        }

        [Fact]
        public void AddUnknownBitToUnknownBitShouldResultInUnknownCarry()
        {
            var value1 = new Integer8Value("0000000?");
            var value2 = new Integer8Value("0000000?");

            value1.Add(value2);
            
            Assert.Equal(0b1111_1100, value1.Mask);
        }
        
        [Fact]
        public void AddKnownBitToUnknownBitsShouldResultInUnknownRippleCarry()
        {
            var value1 = new Integer8Value("000000??");
            var value2 = new Integer8Value("00000001");

            value1.Add(value2);
            
            Assert.Equal(0b1111_1000, value1.Mask);
        }
        
        [Fact]
        public void AddKnownBitToKnownBitsThatCarryOverToUnknownBitsShouldRippleCarry()
        {
            var value1 = new Integer8Value("0000??11");
            var value2 = new Integer8Value("00000001");
            
            value1.Add(value2);
            
            Assert.Equal(0b1110_0011, value1.Mask);
        }
        
        [Fact]
        public void AddUnknownBitToKnownBitsThatCarryOverToUnknownBitsShouldRippleCarry()
        {
            var value1 = new Integer8Value("0000??11");
            var value2 = new Integer8Value("0000000?");

            value1.Add(value2);
            
            Assert.Equal(0b1110_0000, value1.Mask);
        }
    }
}