using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Integer64Tests
    {
        [Fact]
        public void KnownValueSignUnsignedSame()
        {
            var value = new Integer64Value(0x12345678_9ABCDEF0);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer64Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x12345678_9ABCDEF0u, value.U64);
            Assert.Equal(0x12345678_9ABCDEF0, value.I64);
        }
        
        [Fact]
        public void KnownValueSignUnsignedDifferent()
        {
            var value = new Integer64Value(0x80000000_00000000);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer64Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x80000000_00000000, value.U64);
            Assert.Equal(-0x80000000_00000000, value.I64);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer64Value(
                0b111111111_11111111_11111111_11111111_11111111_11111111_11111111_1111111, 
                0b000000000_11111111_00001111_11110000_00110011_11001100_01010101_1010101);
            
            Assert.False(value.IsKnown);
            Assert.Equal(0b000000000_11111111_00001111_11110000_00110011_11001100_01010101_1010101u, value.U64);
        }

        [Theory]
        [InlineData(3, null)]
        [InlineData(5, true)]
        [InlineData(6, false)]
        public void ReadBit(int index, bool? expected)
        {
            var value = new Integer64Value(
                0b00000000_00100000,
                0b11111111_11110000);
            Assert.Equal(expected, value.GetBit(index));
        }

        [Theory]
        [InlineData(63, null)]
        [InlineData(63, true)]
        [InlineData(63, false)]
        public void SetBit(int index, bool? expected)
        {
            var value = new Integer64Value(0);
            value.SetBit(index, expected);
            Assert.Equal(expected, value.GetBit(index));
        }

        [Theory]
        [InlineData("0000000000000000000000000000000000000000000000000000000000000000", true)]
        [InlineData("0000000000000000000000000000000000000000000000000000000000001010", false)]
        [InlineData("0000101000000000000000000000000000000000000000000000000000000000", false)]
        [InlineData("000000000000000000000000000000000000000000000000000000000000?0?0", null)]
        [InlineData("00?00?0000000000000000000000000000000000000000000000000000000000", null)]
        [InlineData("10?00?0000000000000000000000000000000000000000000000000000000000", false)]
        public void IsZero(string input, bool? expected)
        {
            var value = new Integer64Value(input);
            Assert.Equal(expected, value.IsZero);
        }

        [Theory]
        [InlineData("0000000000000000000000000000000000000000000000000000000000000000", false)]
        [InlineData("0000000000000000000000000000000000000000000000000000000000001010", true)]
        [InlineData("0000000000000000000000000000000000001010000000000000000000000000", true)]
        [InlineData("000000000000000000000000000000000000000000000000000000000000?0?0", null)]
        [InlineData("00?00?0000000000000000000000000000000000000000000000000000000000", null)]
        [InlineData("10?00?0000000000000000000000000000000000000000000000000000000000", true)]
        public void IsNonZero(string input, bool? expected)
        {
            var value = new Integer64Value(input);
            Assert.Equal(expected, value.IsNonZero);
        }

        [Theory]
        [InlineData("00000000", "11111111")]
        [InlineData("11111111", "00000000")]
        [InlineData("????????", "????????")]
        [InlineData("0011??00", "1100??00")]
        public void Not(string input, string expected)
        {
            var value1 = new Integer64Value(input);
            
            value1.Not();
            
            Assert.Equal(new Integer64Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "00110101")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000?000", "0000?00?")]
        public void And(string a, string b, string expected)
        {
            var value1 = new Integer64Value(a);
            var value2 = new Integer64Value(b);
            
            value1.And(value2);
            
            Assert.Equal(new Integer64Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11111111")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0001000?", "0011000?")]
        public void Or(string a, string b, string expected)
        {
            var value1 = new Integer64Value(a);
            var value2 = new Integer64Value(b);
            
            value1.Or(value2);
            
            Assert.Equal(new Integer64Value(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11111111")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0011000?", "0001000?")]
        public void Xor(string a, string b, string expected)
        {
            var value1 = new Integer64Value(a);
            var value2 = new Integer64Value(b);
            
            value1.Or(value2);
            
            Assert.Equal(new Integer64Value(expected), value1);
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
            var value1 = new Integer64Value(a);
            var value2 = new Integer64Value(b);

            value1.Add(value2);
            
            Assert.Equal(new Integer64Value(expected), value1);
        }
    }

}