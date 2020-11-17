using Echo.Concrete.Values.ValueType;
using Echo.Core;
using Xunit;
using static Echo.Core.TrileanValue;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Integer16ValueTests
    {
        [Fact]
        public void KnownValueSignUnsignedSame()
        {
            var value = new Integer16Value(0x1234);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer16Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x1234, value.U16);
            Assert.Equal(0x1234, value.I16);
        }
        
        [Fact]
        public void KnownValueSignUnsignedDifferent()
        {
            var value = new Integer16Value(0x8000);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer16Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x8000, value.U16);
            Assert.Equal(-0x8000, value.I16);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer16Value(
                0b00110011_00110011, 
                0b00110011_11001100);
            
            Assert.False(value.IsKnown);
            Assert.Equal(0b00110011_00000000, value.U16);
        }

        [Theory]
        [InlineData(3, Unknown)]
        [InlineData(5, True)]
        [InlineData(6, False)]
        public void ReadBit(int index, TrileanValue expected)
        {
            var value = new Integer16Value(
                0b00000000_00100000,
                0b11111111_11110000);
            Assert.Equal(expected, value.GetBit(index));
        }

        [Theory]
        [InlineData(15, Unknown)]
        [InlineData(15, True)]
        [InlineData(15, False)]
        public void SetBit(int index, TrileanValue expected)
        {
            var value = new Integer16Value(0);
            value.SetBit(index, expected);
            Assert.Equal(expected, value.GetBit(index));
        }

        [Theory]
        [InlineData("0000000000000000", True)]
        [InlineData("0000000000001010", False)]
        [InlineData("0000101000000000", False)]
        [InlineData("000000000000?0?0", Unknown)]
        [InlineData("00?00?0000000000", Unknown)]
        [InlineData("10?00?0000000000", False)]
        public void IsZero(string input, TrileanValue expected)
        {
            var value = new Integer16Value(input);
            
            Assert.Equal(expected, value.IsZero);
        }

        [Theory]
        [InlineData("0000000000000000", False)]
        [InlineData("0000000000001010", True)]
        [InlineData("0000101000000000", True)]
        [InlineData("000000000000?0?0", Unknown)]
        [InlineData("00?00?0000000000", Unknown)]
        [InlineData("10?00?0000000000", True)]
        public void IsNonZero(string input, TrileanValue expected)
        {
            var value = new Integer16Value(input);
            
            Assert.Equal(expected, value.IsNonZero);
        }

        [Theory]
        [InlineData("1111111100000000", "0000000011111111")]
        [InlineData("0000000011111111", "1111111100000000")]
        [InlineData("????????????????", "????????????????")]
        [InlineData("000000000011??00", "111111111100??11")]
        public void Not(string input, string expected)
        {
            var value1 = new Integer16Value(input);
            
            value1.Not();
            
            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "00100101")]
        [InlineData("00000000", "0000000?", "00000000")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000?000", "00000000")]
        public void And(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);
            
            value1.And(value2);
            
            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11111111")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0001000?", "0011000?")]
        public void Or(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);
            
            value1.Or(value2);
            
            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11011010")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0011000?", "0001000?")]
        public void Xor(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);
            
            value1.Xor(value2);
            
            Assert.Equal(new Integer16Value(expected), value1);
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
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            value1.Add(value2);
            
            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("0000000000001111", "0000000000001000", "0000000000000111")]
        [InlineData("0000000000001000", "0000000000000001", "0000000000000111")]
        [InlineData("0000000000001???", "0000000000000001", "000000000000????")]
        [InlineData("0000000000001???", "000000000000000?", "000000000000????")]
        [InlineData("0000000000000000", "000000000000000?", "????????????????")]
        public void Subtract(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            value1.Subtract(value2);

            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("0000000000000000", "0000000000000000", "0000000000000000")]
        [InlineData("0000000000000001", "0000000000000000", "0000000000000000")]
        [InlineData("0000000000000011", "0000000000000010", "0000000000000110")]
        [InlineData("00000000000000?1", "0000000000000010", "0000000000000?10")]
        [InlineData("0000000000001001", "0000000000110011", "0000000111001011")]
        public void Multiply(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            value1.Multiply(value2);

            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("00001111", "00001000", "00000001")]
        [InlineData("00001000", "00000001", "00001000")]
        [InlineData("00001???", "00000001", "0000????")]
        [InlineData("00001???", "0000000?", "0000????")]
        [InlineData("1111111100000000", "0000000011111111", "0000000100000000")]
        public void Divide(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            value1.Divide(value2);

            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("00001111", "00001000", "00000111")]
        [InlineData("00001000", "00000001", "00000000")]
        [InlineData("00001???", "00000001", "0000000?")]
        [InlineData("00001???", "0000000?", "0000000?")]
        [InlineData("1111111100000000", "0000000011111110", "0000000000000010")]
        public void Remainder(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            value1.Remainder(value2);

            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("01010101", "01010101", True)]
        [InlineData("01010101", "10101010", False)]
        [InlineData("010?0101", "01010101", Unknown)]
        [InlineData("010?0111", "01010101", False)]
        public void IsEqualTo(string a, string b, TrileanValue expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            Assert.Equal(expected, value1.IsEqualTo(value2));
        }

        [Theory]
        [InlineData("00000000", "00000000", false, False)]
        [InlineData("00000000", "00000001", false, True)]
        [InlineData("00000001", "00010000", false, True)]
        [InlineData("00000001", "00000000", false, False)]
        [InlineData("0000000?", "00000000", false, False)]
        [InlineData("0000001?", "000101??", false, True)]
        [InlineData("000101??", "0000001?", false, False)]
        [InlineData("0000000?", "000?0000", false, Unknown)]
        public void IsLessThan(string a, string b, bool signed, TrileanValue expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            Assert.Equal(expected, value1.IsLessThan(value2, signed));
        }

        [Theory]
        [InlineData("00000000", "00000000", false, False)]
        [InlineData("00000001", "00000000", false, True)]
        [InlineData("00000001", "00010000", false, False)]
        [InlineData("00010000", "00000001", false, True)]
        [InlineData("0001000?", "00001000", false, True)]
        [InlineData("000101??", "0000001?", false, True)]
        public void IsGreaterThan(string a, string b, bool signed, TrileanValue expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            Assert.Equal(expected, value1.IsGreaterThan(value2, signed));
        }
    }

}