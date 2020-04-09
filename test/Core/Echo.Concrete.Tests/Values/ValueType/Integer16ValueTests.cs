using Echo.Concrete.Values.ValueType;
using Xunit;

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
        [InlineData(3, null)]
        [InlineData(5, true)]
        [InlineData(6, false)]
        public void ReadBit(int index, bool? expected)
        {
            var value = new Integer16Value(
                0b00000000_00100000,
                0b11111111_11110000);
            Assert.Equal(expected, value.GetBit(index));
        }

        [Theory]
        [InlineData(15, null)]
        [InlineData(15, true)]
        [InlineData(15, false)]
        public void SetBit(int index, bool? expected)
        {
            var value = new Integer16Value(0);
            value.SetBit(index, expected);
            Assert.Equal(expected, value.GetBit(index));
        }

        [Theory]
        [InlineData("0000000000000000", true)]
        [InlineData("0000000000001010", false)]
        [InlineData("0000101000000000", false)]
        [InlineData("000000000000?0?0", null)]
        [InlineData("00?00?0000000000", null)]
        [InlineData("10?00?0000000000", false)]
        public void IsZero(string input, bool? expected)
        {
            var value = new Integer16Value(input);
            
            Assert.Equal(expected, value.IsZero);
        }

        [Theory]
        [InlineData("0000000000000000", false)]
        [InlineData("0000000000001010", true)]
        [InlineData("0000101000000000", true)]
        [InlineData("000000000000?0?0", null)]
        [InlineData("00?00?0000000000", null)]
        [InlineData("10?00?0000000000", true)]
        public void IsNonZero(string input, bool? expected)
        {
            var value = new Integer16Value(input);
            
            Assert.Equal(expected, value.IsNonZero);
        }

        [Theory]
        [InlineData("00000000", "11111111")]
        [InlineData("11111111", "00000000")]
        [InlineData("????????", "????????")]
        [InlineData("0011??00", "1100??00")]
        public void Not(string input, string expected)
        {
            var value1 = new Integer16Value(input);
            
            value1.Not();
            
            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "00110101")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000?000", "0000?00?")]
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
        [InlineData("00110101", "11101111", "11111111")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0011000?", "0001000?")]
        public void Xor(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);
            
            value1.Or(value2);
            
            Assert.Equal(new Integer16Value(expected), value1);
        }

        [Theory]
        [InlineData("00010010", "00110100", "01000110")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("00000001", "0000000?", "000000??")]
        [InlineData("0000000?", "00000001", "000000??")]
        [InlineData("0000000?", "0000000?", "000000??")]
        [InlineData("0000??11", "00000001", "000?????")]
        [InlineData("000??0??", "00000101", "00??????")]
        public void Add(string a, string b, string expected)
        {
            var value1 = new Integer16Value(a);
            var value2 = new Integer16Value(b);

            value1.Add(value2);
            
            Assert.Equal(new Integer16Value(expected), value1);
        }
    }

}