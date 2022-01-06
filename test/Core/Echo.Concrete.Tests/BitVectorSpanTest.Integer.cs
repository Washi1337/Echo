using Echo.Core;
using Xunit;

namespace Echo.Concrete.Tests
{
    public partial class BitVectorSpanTest
    {
        [Theory]
        [InlineData("00000000", "00000001", TrileanValue.False)]
        [InlineData("00000001", "00000010", TrileanValue.False)]
        [InlineData("0000000?", "000000??", TrileanValue.False)]
        [InlineData("00000010", "00000011", TrileanValue.False)]
        [InlineData("00000111", "00001000", TrileanValue.False)]
        [InlineData("01000111", "01001000", TrileanValue.False)]
        [InlineData("11111111", "00000000", TrileanValue.True)]
        [InlineData("?1111111", "?0000000", TrileanValue.Unknown)]
        public void IntegerIncrement(string input, string expectedResult, TrileanValue expectedCarry)
        {
            var value = BitVector.ParseBinary(input).AsSpan();
            var carry = value.IntegerIncrement();
            Assert.Equal(expectedResult, value.ToBitString());
            Assert.Equal(expectedCarry, carry);
        }

        [Theory]
        [InlineData("00000000", "00000000")]
        [InlineData("00000001", "11111111")]
        [InlineData("11111111", "00000001")]
        [InlineData("01111111", "10000001")]
        [InlineData("10000001", "01111111")]
        public void IntegerNegate(string input, string expected)
        {
            var value = BitVector.ParseBinary(input).AsSpan();
            value.IntegerNegate();
            Assert.Equal(expected, value.ToBitString());
        }
        
        [Theory]
        [InlineData("00000001", "00000000", TrileanValue.False)]
        [InlineData("00000010", "00000001", TrileanValue.False)]
        [InlineData("00000011", "00000010", TrileanValue.False)]
        [InlineData("00000?01", "00000?00", TrileanValue.False)]
        [InlineData("00001000", "00000111", TrileanValue.False)]
        [InlineData("00000???", "????????", TrileanValue.Unknown)]
        [InlineData("00001???", "0000????", TrileanValue.False)]
        [InlineData("00000000", "11111111", TrileanValue.True)]
        public void IntegerDecrement(string input, string expectedValue, TrileanValue expectedBorrow)
        {
            var value = BitVector.ParseBinary(input).AsSpan();
            var borrow = value.IntegerDecrement();
            Assert.Equal(expectedValue, value.ToBitString());
            Assert.Equal(expectedBorrow, borrow);
        }
        
        [Theory]
        // Basic truth table tests.
        [InlineData("00000000", "00000000", "00000000", TrileanValue.False)]
        [InlineData("00000000", "00000001", "00000001", TrileanValue.False)]
        [InlineData("00000000", "0000000?", "0000000?", TrileanValue.False)]
        [InlineData("00000001", "00000000", "00000001", TrileanValue.False)]
        [InlineData("00000001", "00000001", "00000010", TrileanValue.False)]
        [InlineData("00000001", "0000000?", "000000??", TrileanValue.False)]
        [InlineData("0000000?", "00000000", "0000000?", TrileanValue.False)]
        [InlineData("0000000?", "00000001", "000000??", TrileanValue.False)]
        [InlineData("0000000?", "0000000?", "000000??", TrileanValue.False)]
        // Some regression
        [InlineData("0000??11", "00000001", "000???00", TrileanValue.False)]
        [InlineData("000??0??", "00000101", "00??????", TrileanValue.False)]
        [InlineData("00010010", "00110100", "01000110", TrileanValue.False)]
        // Overflow tests
        [InlineData("11111111", "00001000", "00000111", TrileanValue.True)]
        [InlineData("?1111111", "00001000", "?0000111", TrileanValue.Unknown)]
        public void IntegerAdd(string a, string b, string expectedValue, TrileanValue expectedCarry)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            var carry = value1.IntegerAdd(value2);
            
            Assert.Equal(expectedValue, value1.ToBitString());
            Assert.Equal(expectedCarry, carry);
        }

        [Theory]
        [InlineData("00001111", "00001000", "00000111", TrileanValue.False)]
        [InlineData("00001000", "00000001", "00000111", TrileanValue.False)]
        [InlineData("00001???", "00000001", "0000????", TrileanValue.False)]
        [InlineData("00001???", "0000000?", "0000????", TrileanValue.False)]
        [InlineData("00000000", "00001000", "11111000", TrileanValue.True)]
        [InlineData("00000000", "0000000?", "????????", TrileanValue.Unknown)]
        public void IntegerSubtract(string a, string b, string expectedValue, TrileanValue expectedBorrow)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            var borrow = value1.IntegerSubtract(value2);

            Assert.Equal(expectedValue, value1.ToBitString());
            Assert.Equal(borrow, expectedBorrow);
        }

    }
}