using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class IntegerNValueTest
    {
        [Theory]
        [InlineData("00000000", true)]
        [InlineData("00001010", false)]
        [InlineData("0000?0?0", null)]
        [InlineData("0100?0?0", false)]
        public void IsZero(string input, bool? expected)
        {
            var value = new IntegerNValue(input);
            
            Assert.Equal(expected, value.IsZero);
        }

        [Theory]
        [InlineData("00000000", false)]
        [InlineData("00001010", true)]
        [InlineData("0000?0?0", null)]
        [InlineData("0100?0?0", true)]
        public void IsNonZero(string input, bool? expected)
        {
            var value = new IntegerNValue(input);
            
            Assert.Equal(expected, value.IsNonZero);
        }

        [Theory]
        [InlineData("00000000", "11111111")]
        [InlineData("11111111", "00000000")]
        [InlineData("????????", "????????")]
        [InlineData("0011??00", "1100??00")]
        public void Not(string input, string expected)
        {
            var value1 = new IntegerNValue(input);
            
            value1.Not();
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "00110101")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000?000", "0000?00?")]
        public void And(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);
            
            value1.And(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11111111")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0001000?", "0011000?")]
        public void Or(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);
            
            value1.Or(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11111111")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0011000?", "0001000?")]
        public void Xor(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);
            
            value1.Or(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00000001", 1, "00000010")]
        [InlineData("0000000?", 1, "000000?0")]
        [InlineData("00000001", 8, "00000000")]
        [InlineData("01101100", 2, "00011011")]
        public void LeftShift(string input, int count, string expected)
        {
            var value = new IntegerNValue(input);
            value.LeftShift(count);
            Assert.Equal(new IntegerNValue(expected), value);
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
            var value = new IntegerNValue(input);
            value.RightShift(count, signExtend);
            Assert.Equal(new IntegerNValue(expected), value);
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
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            value1.Add(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00000000", "00000000")]
        [InlineData("00000001", "11111111")]
        [InlineData("11111111", "00000001")]
        [InlineData("01111111", "10000001")]
        [InlineData("10000001", "01111111")]
        public void TwosComplement(string input, string expected)
        {
            var value = new IntegerNValue(input);
            value.TwosComplement();
            Assert.Equal(new IntegerNValue(expected), value);
        }

        [Theory]
        [InlineData("00001111", "00001000", "00000111")]
        [InlineData("00001000", "00000001", "00000111")]
        [InlineData("00001???", "00000001", "00000???")]
        [InlineData("00001???", "0000000?", "0000????")]
        [InlineData("00000000", "0000000?", "????????")]
        public void Subtract(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            value1.Subtract(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }
    }
}