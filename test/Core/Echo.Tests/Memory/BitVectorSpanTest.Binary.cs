using Echo.Memory;
using Xunit;

namespace Echo.Tests.Memory
{
    public partial class BitVectorSpanTest
    {
        [Theory]
        [InlineData("00000000", "11111111")]
        [InlineData("11111111", "00000000")]
        [InlineData("????????", "????????")]
        [InlineData("0011??00", "1100??11")]
        public void Not(string input, string expected)
        {
            var value1 = BitVector.ParseBinary(input).AsSpan();
            value1.Not();
            Assert.Equal(expected, value1.ToBitString());
        }
        
        [Theory]
        [InlineData("00110101", "11101111", "00100101")]
        [InlineData("00000000", "0000000?", "00000000")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000?000", "00000000")]
        [InlineData("00000001", "0000000?", "0000000?")]
        public void And(string a, string b, string expected)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();
            
            value1.And(value2);
            
            Assert.Equal(expected, value1.ToBitString());
        }
        
        [Theory]
        [InlineData("00110101", "11101111", "11111111")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0001000?", "0011000?")]
        [InlineData("00000001", "0000000?", "00000001")]
        public void Or(string a, string b, string expected)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();
            
            value1.Or(value2);
            
            Assert.Equal(expected, value1.ToBitString());
        }
        
        [Theory]
        [InlineData("00110101", "11101111", "11011010")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0011000?", "0001000?")]
        public void Xor(string a, string b, string expected)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();
            
            value1.Xor(value2);
            
            Assert.Equal(expected, value1.ToBitString());
        }
        
        [Theory]
        [InlineData("00000001", 1, "00000010")]
        [InlineData("0000000?", 1, "000000?0")]
        [InlineData("00000001", 8, "00000000")]
        [InlineData("01101100", 2, "10110000")]
        public void ShiftLeft(string input, int count, string expected)
        {
            var value = BitVector.ParseBinary(input).AsSpan();
            value.ShiftLeft(count);
            Assert.Equal(expected, value.ToBitString());
        }

        [Theory]
        [InlineData("00000001", 1, false, "00000000")]
        [InlineData("10000000", 1, false, "01000000")]
        [InlineData("10000000", 1, true, "11000000")]
        [InlineData("?0000000", 1, false, "0?000000")]
        [InlineData("?0000000", 1, true, "??000000")]
        [InlineData("01101100", 2, false, "00011011")]
        [InlineData("10000000", 8, false, "00000000")]
        public void ShiftRight(string input, int count, bool signExtend, string expected)
        {
            var value = BitVector.ParseBinary(input).AsSpan();
            value.ShiftRight(count, signExtend);
            Assert.Equal(expected, value.ToBitString());
        }
    }
}