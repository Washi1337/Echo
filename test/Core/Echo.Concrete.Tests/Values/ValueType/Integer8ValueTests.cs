using System;
using System.Numerics;
using Echo.Concrete.Values.ValueType;
using Echo.Core;
using Xunit;
using static Echo.Core.TrileanValue;

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

            var bits = new byte[1];
            value.GetBits(bits);
            Assert.Equal(0b0001_0010, bits[0]);
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

        [Theory]
        [InlineData("00000000", True)]
        [InlineData("00001010", False)]
        [InlineData("0000?0?0", Unknown)]
        [InlineData("0100?0?0", False)]
        public void IsZero(string input, TrileanValue expected)
        {
            var value = new Integer8Value(input);
            
            Assert.Equal(expected, value.IsZero);
        }

        [Theory]
        [InlineData("00000000", False)]
        [InlineData("00001010", True)]
        [InlineData("0000?0?0", Unknown)]
        [InlineData("0100?0?0", True)]
        public void IsNonZero(string input, TrileanValue expected)
        {
            var value = new Integer8Value(input);
            
            Assert.Equal(expected, value.IsNonZero);
        }

        [Theory]
        [InlineData("00000000", "11111111")]
        [InlineData("11111111", "00000000")]
        [InlineData("????????", "????????")]
        [InlineData("0011??00", "1100??11")]
        public void Not(string input, string expected)
        {
            var value1 = new Integer8Value(input);
            
            value1.Not();
            
            Assert.Equal(new Integer8Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "00100101")]
        [InlineData("00000000", "0000000?", "00000000")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000?000", "00000000")]
        public void And(string a, string b, string expected)
        {
            var value1 = new Integer8Value(a);
            var value2 = new Integer8Value(b);
            
            value1.And(value2);
            
            Assert.Equal(new Integer8Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11111111")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0001000?", "0011000?")]
        public void Or(string a, string b, string expected)
        {
            var value1 = new Integer8Value(a);
            var value2 = new Integer8Value(b);
            
            value1.Or(value2);
            
            Assert.Equal(new Integer8Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11011010")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0011000?", "0001000?")]
        public void Xor(string a, string b, string expected)
        {
            var value1 = new Integer8Value(a);
            var value2 = new Integer8Value(b);
            
            value1.Xor(value2);
            
            Assert.Equal(new Integer8Value(expected), value1);
        }

        [Theory]
        [InlineData("00000001", 1, "00000010")]
        [InlineData("0000000?", 1, "000000?0")]
        [InlineData("00000001", 8, "00000000")]
        [InlineData("01101100", 2, "10110000")]
        public void LeftShift(string input, int count, string expected)
        {
            var value = new Integer8Value(input);
            value.LeftShift(count);
            Assert.Equal(new Integer8Value(expected), value);
        }

        [Theory]
        [InlineData("00000001", 1, false, "00000000")]
        [InlineData("10000000", 1, false, "01000000")]
        [InlineData("10000000", 1, true, "11000000")]
        [InlineData("?0000000", 1, false, "0?000000")]
        [InlineData("?0000000", 1, true, "??000000")]
        [InlineData("10000000", 8, false, "00000000")]
        public void RightShift(string input, int count, bool signExtend, string expected)
        {
            var value = new Integer8Value(input);
            value.RightShift(count, signExtend);
            Assert.Equal(new Integer8Value(expected), value);
        }

        [Theory]
        [InlineData("00010010", "00110100", "01000110")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("00000001", "0000000?", "000000??")]
        [InlineData("0000000?", "00000001", "000000??")]
        [InlineData("0000000?", "0000000?", "000000??")]
        [InlineData("0000??11", "00000001", "000???00")]
        [InlineData("000??0??", "00000101", "00??????")]
        public void Add(string a, string b, string expected)
        {
            var value1 = new Integer8Value(a);
            var value2 = new Integer8Value(b);

            value1.Add(value2);
            
            Assert.Equal(new Integer8Value(expected), value1);
        }

        [Theory]
        [InlineData("00000000", "00000000", "00000000")]
        [InlineData("00000001", "00000000", "00000000")]
        [InlineData("00000011", "00000010", "00000110")]
        [InlineData("000000?1", "00000010", "00000?10")]
        [InlineData("00001001", "00110011", "11001011")]
        public void Multiply(string a, string b, string expected)
        {
            var value1 = new Integer8Value(a);
            var value2 = new Integer8Value(b);

            value1.Multiply(value2);
            
            Assert.Equal(new Integer8Value(expected), value1);
        }

        [Theory]
        [InlineData("00000000", "00000000")]
        [InlineData("00000001", "11111111")]
        [InlineData("11111111", "00000001")]
        [InlineData("01111111", "10000001")]
        [InlineData("10000001", "01111111")]
        public void TwosComplement(string input, string expected)
        {
            var value = new Integer8Value(input);
            value.TwosComplement();
            Assert.Equal(new Integer8Value(expected), value);
        }

        [Theory]
        [InlineData("00001111", "00001000", "00000111")]
        [InlineData("00001000", "00000001", "00000111")]
        [InlineData("00001???", "00000001", "0000????")]
        [InlineData("00001???", "0000000?", "0000????")]
        [InlineData("00000000", "0000000?", "????????")]
        public void Subtract(string a, string b, string expected)
        {
            var value1 = new Integer8Value(a);
            var value2 = new Integer8Value(b);

            value1.Subtract(value2);
            
            Assert.Equal(new Integer8Value(expected), value1);
        }
        
    }
}