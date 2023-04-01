using Echo.Core;
using Xunit;
using static Echo.Core.TrileanValue;

namespace Echo.Concrete.Tests
{
    public partial class BitVectorSpanTest
    {
        [Theory]
        [InlineData("00000000", "00000001", False)]
        [InlineData("00000001", "00000010", False)]
        [InlineData("0000000?", "000000??", False)]
        [InlineData("00000010", "00000011", False)]
        [InlineData("00000111", "00001000", False)]
        [InlineData("01000111", "01001000", False)]
        [InlineData("11111111", "00000000", True)]
        [InlineData("?1111111", "?0000000", Unknown)]
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
        [InlineData("00000001", "00000000", False)]
        [InlineData("00000010", "00000001", False)]
        [InlineData("00000011", "00000010", False)]
        [InlineData("00000?01", "00000?00", False)]
        [InlineData("00001000", "00000111", False)]
        [InlineData("00000???", "????????", Unknown)]
        [InlineData("00001???", "0000????", False)]
        [InlineData("00000000", "11111111", True)]
        public void IntegerDecrement(string input, string expectedValue, TrileanValue expectedBorrow)
        {
            var value = BitVector.ParseBinary(input).AsSpan();
            var borrow = value.IntegerDecrement();
            Assert.Equal(expectedValue, value.ToBitString());
            Assert.Equal(expectedBorrow, borrow);
        }
        
        [Theory]
        // Basic truth table tests.
        [InlineData("00000000", "00000000", "00000000", False)]
        [InlineData("00000000", "00000001", "00000001", False)]
        [InlineData("00000000", "0000000?", "0000000?", False)]
        [InlineData("00000001", "00000000", "00000001", False)]
        [InlineData("00000001", "00000001", "00000010", False)]
        [InlineData("00000001", "0000000?", "000000??", False)]
        [InlineData("0000000?", "00000000", "0000000?", False)]
        [InlineData("0000000?", "00000001", "000000??", False)]
        [InlineData("0000000?", "0000000?", "000000??", False)]
        // Some regression
        [InlineData("0000??11", "00000001", "000???00", False)]
        [InlineData("000??0??", "00000101", "00??????", False)]
        [InlineData("00010010", "00110100", "01000110", False)]
        // Overflow tests
        [InlineData("11111111", "00001000", "00000111", True)]
        [InlineData("?1111111", "00001000", "?0000111", Unknown)]
        public void IntegerAdd(string a, string b, string expectedValue, TrileanValue expectedCarry)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            var carry = value1.IntegerAdd(value2);
            
            Assert.Equal(expectedValue, value1.ToBitString());
            Assert.Equal(expectedCarry, carry);
        }

        [Theory]
        [InlineData("00001111", "00001000", "00000111", False)]
        [InlineData("00001000", "00000001", "00000111", False)]
        [InlineData("00001???", "00000001", "0000????", False)]
        [InlineData("00001???", "0000000?", "0000????", False)]
        [InlineData("00000000", "00001000", "11111000", True)]
        [InlineData("00000000", "0000000?", "????????", Unknown)]
        public void IntegerSubtract(string a, string b, string expectedValue, TrileanValue expectedBorrow)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            var borrow = value1.IntegerSubtract(value2);

            Assert.Equal(expectedValue, value1.ToBitString());
            Assert.Equal(borrow, expectedBorrow);
        }

        [Theory]
        [InlineData("00000000", "00000000", "00000000", False)]
        [InlineData("00000001", "00000000", "00000000", False)]
        [InlineData("00000001", "0000000?", "0000000?", False)]
        [InlineData("0000000?", "00000001", "0000000?", False)]
        [InlineData("00000011", "00000010", "00000110", False)]
        [InlineData("000000?1", "00000010", "00000?10", False)]
        [InlineData("00001001", "00110011", "11001011", True)]
        [InlineData("11111111", "00000010", "11111110", True)]
        public void IntegerMultiply(string a, string b, string expectedValue, TrileanValue expectedCarry)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            var carry = value1.IntegerMultiply(value2);
            
            Assert.Equal(expectedValue, value1.ToBitString());
            Assert.Equal(expectedCarry, carry);
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
        public void IntegerIsLessThan(string a, string b, bool signed, TrileanValue expected)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            Assert.Equal(expected, value1.IntegerIsLessThan(value2, signed));
        }

        [Theory]
        [InlineData("00000000", "00000000", false, False)]
        [InlineData("00000001", "00000000", false, True)]
        [InlineData("00000001", "00010000", false, False)]
        [InlineData("00010000", "00000001", false, True)]
        [InlineData("0001000?", "00001000", false, True)]
        [InlineData("000101??", "0000001?", false, True)]
        public void IntegerIsGreaterThan(string a, string b, bool signed, TrileanValue expected)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            Assert.Equal(expected, value1.IntegerIsGreaterThan(value2, signed));
        }
        
        [Theory]
        [InlineData("00000000", "00000000", false, True)]
        [InlineData("00000001", "00000000", false, True)]
        [InlineData("0000000?", "00000000", false, True)]
        [InlineData("00000000", "00000001", false, False)]
        [InlineData("00000001", "00000001", false, True)]
        [InlineData("0000000?", "00000001", false, Unknown)]
        [InlineData("00000000", "0000000?", false, Unknown)]
        [InlineData("00000001", "0000000?", false, True)]
        [InlineData("0000000?", "0000000?", false, Unknown)]
        [InlineData("00000010", "00000010", false, True)]
        [InlineData("00000010", "00000011", false, False)]
        [InlineData("00000010", "0000001?", false, Unknown)]
        [InlineData("00000010", "000000?0", false, True)]
        [InlineData("00000010", "000000?1", false, Unknown)]
        [InlineData("00000010", "000000??", false, Unknown)]
        [InlineData("00000011", "000000?0", false, True)]
        [InlineData("00000011", "000000?1", false, True)]
        [InlineData("00000011", "000000??", false, True)]
        [InlineData("000000?0", "00000000", false, True)]
        [InlineData("000000?0", "00000001", false, Unknown)]
        [InlineData("000000?0", "0000000?", false, Unknown)]
        [InlineData("00001000", "00001???", false, Unknown)]
        public void IntegerIsGreaterThanOrEqual(string a, string b, bool signed, TrileanValue expected)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            Assert.Equal(expected, value1.IntegerIsGreaterThanOrEqual(value2, signed));
        }
        
        [Theory]
        [InlineData("00000000", "00000000", false, True)]
        [InlineData("00000000", "00000001", false, True)]
        [InlineData("00000000", "0000000?", false, True)]
        [InlineData("00000001", "00000000", false, False)]
        [InlineData("00000001", "00000001", false, True)]
        [InlineData("00000001", "0000000?", false, Unknown)]
        [InlineData("0000000?", "00000000", false, Unknown)]
        [InlineData("0000000?", "00000001", false, True)]
        [InlineData("0000000?", "0000000?", false, Unknown)]
        [InlineData("00000010", "00000010", false, True)]
        [InlineData("00000011", "00000010", false, False)]
        [InlineData("0000001?", "00000010", false, Unknown)]
        [InlineData("000000?0", "00000010", false, True)]
        [InlineData("000000?1", "00000010", false, Unknown)]
        [InlineData("000000??", "00000010", false, Unknown)]
        [InlineData("000000?0", "00000011", false, True)]
        [InlineData("000000?1", "00000011", false, True)]
        [InlineData("000000??", "00000011", false, True)]
        [InlineData("00000000", "000000?0", false, True)]
        [InlineData("00000001", "000000?0", false, Unknown)]
        [InlineData("0000000?", "000000?0", false, Unknown)]
        [InlineData("00001000", "00001???", false, True)]
        public void IntegerIsLessThanOrEqual(string a, string b, bool signed, TrileanValue expected)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            Assert.Equal(expected, value1.IntegerIsLessThanOrEqual(value2, signed));
        }
    }
}