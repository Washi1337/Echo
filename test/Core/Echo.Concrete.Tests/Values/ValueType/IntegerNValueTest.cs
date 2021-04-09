using Echo.Concrete.Values.ValueType;
using Echo.Core;
using System;
using Xunit;
using static Echo.Core.TrileanValue;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class IntegerNValueTest
    {
        [Theory]
        [InlineData("00000000", True)]
        [InlineData("00001010", False)]
        [InlineData("0000?0?0", Unknown)]
        [InlineData("0100?0?0", False)]
        public void IsZero(string input, TrileanValue expected)
        {
            var value = new IntegerNValue(input);
            
            Assert.Equal(expected, value.IsZero);
        }

        [Theory]
        [InlineData("00000000", False)]
        [InlineData("00001010", True)]
        [InlineData("0000?0?0", Unknown)]
        [InlineData("0100?0?0", True)]
        public void IsNonZero(string input, TrileanValue expected)
        {
            var value = new IntegerNValue(input);
            
            Assert.Equal(expected, value.IsNonZero);
        }

        [Theory]
        [InlineData("00000000", "11111111")]
        [InlineData("11111111", "00000000")]
        [InlineData("????????", "????????")]
        [InlineData("0011??00", "1100??11")]
        public void Not(string input, string expected)
        {
            var value1 = new IntegerNValue(input);
            
            value1.Not();
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "00100101")]
        [InlineData("00000000", "0000000?", "00000000")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000?000", "00000000")]
        [InlineData("00000001", "0000000?", "0000000?")]
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
        [InlineData("00000001", "0000000?", "00000001")]
        public void Or(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);
            
            value1.Or(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00110101", "11101111", "11011010")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("0000000?", "0000000?", "0000000?")]
        [InlineData("0010000?", "0011000?", "0001000?")]
        public void Xor(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);
            
            value1.Xor(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00000001", 1, "00000010")]
        [InlineData("0000000?", 1, "000000?0")]
        [InlineData("00000001", 8, "00000000")]
        [InlineData("01101100", 2, "10110000")]
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
        [InlineData("01101100", 2, false, "00011011")]
        [InlineData("10000000", 8, false, "00000000")]
        public void RightShift(string input, int count, bool signExtend, string expected)
        {
            var value = new IntegerNValue(input);
            value.RightShift(count, signExtend);
            Assert.Equal(new IntegerNValue(expected), value);
        }

        [Theory]
        
        // Basic truth table tests.
        [InlineData("00000000", "00000000", "00000000")]
        [InlineData("00000000", "00000001", "00000001")]
        [InlineData("00000000", "0000000?", "0000000?")]
        [InlineData("00000001", "00000000", "00000001")]
        [InlineData("00000001", "00000001", "00000010")]
        [InlineData("00000001", "0000000?", "000000??")]
        [InlineData("0000000?", "00000000", "0000000?")]
        [InlineData("0000000?", "00000001", "000000??")]
        [InlineData("0000000?", "0000000?", "000000??")]
        
        // Some regression
        [InlineData("0000??11", "00000001", "000???00")]
        [InlineData("000??0??", "00000101", "00??????")]
        [InlineData("00010010", "00110100", "01000110")]
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
        [InlineData("00001???", "00000001", "0000????")]
        [InlineData("00001???", "0000000?", "0000????")]
        [InlineData("00000000", "0000000?", "????????")]
        public void Subtract(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            value1.Subtract(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00000000", "00000000", "00000000")]
        [InlineData("00000001", "00000000", "00000000")]
        [InlineData("00000001", "0000000?", "0000000?")]
        [InlineData("0000000?", "00000001", "0000000?")]
        [InlineData("00000011", "00000010", "00000110")]
        [InlineData("000000?1", "00000010", "00000?10")]
        [InlineData("00001001", "00110011", "11001011")]
        public void Multiply(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            value1.Multiply(value2);
            
            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00000001", "00000001", "00000001")]
        [InlineData("00000001", "0000000?", "0000000?")]
        [InlineData("0000000?", "00000001", "0000000?")]
        [InlineData("00000111", "00000010", "00000011")]
        [InlineData("000000?1", "00000010", "0000000?")]
        [InlineData("00001001", "00000011", "00000011")]
        public void Divide(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            value1.Divide(value2);

            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00000001", "00000000")]
        [InlineData("0000000?", "00000000")]
        public void Divide_DividingByZero(string a, string b)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            Assert.Throws<DivideByZeroException>(() => value1.Divide(value2));
        }

        [Theory]
        [InlineData("000000?1000000?1", "00000010")]
        [InlineData("00001001", "0000000110000011")]
        public void Divide_DifferentSizes(string a, string b)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);
            bool exceptionThrowed = false;
            try
            {
                value1.Divide(value2);
            }
            catch (ArgumentException)
            {
                exceptionThrowed = true;
            }

            Assert.True(exceptionThrowed);
        }

        [Theory]
        [InlineData("00000001", "00000001", "00000000")]
        [InlineData("00000001", "0000000?", "0000000?")]
        [InlineData("0000000?", "00000001", "0000000?")]
        [InlineData("00000111", "00000010", "00000001")]
        [InlineData("000000?1", "00000010", "000000??")]
        [InlineData("00001001", "00000011", "00000000")]
        public void Remainder(string a, string b, string expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            value1.Remainder(value2);

            Assert.Equal(new IntegerNValue(expected), value1);
        }

        [Theory]
        [InlineData("00000001", "00000000")]
        [InlineData("0000000?", "00000000")]
        public void Remainder_DividingByZero(string a, string b)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            Assert.Throws<DivideByZeroException>(() => value1.Divide(value2));
        }

        [Theory]
        [InlineData("000000?1000000?1", "00000010")]
        [InlineData("00001001", "0000000110000011")]
        public void Remainder_DifferentSizes(string a, string b)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            Assert.Throws<ArgumentException>(() => value1.Divide(value2));
        }

        [Fact]
        public void Extend8BitsTo8Bits()
        {
            var value = new IntegerNValue("00110??0");
            var newValue = value.Extend(8, false);
            
            var int8Value = Assert.IsAssignableFrom<Integer8Value>(newValue);
            Assert.Equal(0b0011_0000, int8Value.U8);
            Assert.Equal(0b1111_1001, int8Value.Mask); 
        }

        [Theory]
        [InlineData("1100??00", false, (ushort) 0b00000000_11000000, 0b11111111_11110011)]
        [InlineData("1100??00", true, (ushort) 0b11111111_11000000, 0b11111111_11110011)]
        [InlineData("0100??00", true, (ushort) 0b00000000_01000000, 0b11111111_11110011)]
        [InlineData("?100??00", false, (ushort) 0b00000000_01000000, 0b11111111_01110011)]
        [InlineData("?100??00", true, (ushort) 0b00000000_01000000, 0b00000000_01110011)]
        public void Extend8BitsTo16Bits(string input, bool signExtend, ushort expectedBits, ushort expectedMask)
        {
            var value = new IntegerNValue(input);
            var newValue = value.Extend(16, signExtend);
                
            var int16Value = Assert.IsAssignableFrom<Integer16Value>(newValue);
            Assert.Equal(expectedBits, int16Value.U16);
            Assert.Equal(expectedMask, int16Value.Mask);
        }

        [Theory]
        [InlineData("1100??00", false, 0b00000000_00000000_00000000_11000000, 0b11111111_11111111_11111111_11110011u)]
        [InlineData("1100??00", true, 0b11111111_11111111_11111111_11000000, 0b11111111_11111111_11111111_11110011u)]
        [InlineData("?100??00", false, 0b00000000_00000000_00000000_01000000, 0b11111111_11111111_11111111_01110011u)]
        [InlineData("?100??00", true, 0b00000000_00000000_00000000_01000000, 0b00000000_00000000_00000000_01110011u)]
        public void Extend8BitsTo32Bits(string input, bool signExtend, uint expectedBits, uint expectedMask)
        {
            var value = new IntegerNValue(input);
            var newValue = value.Extend(32, signExtend);
                
            var int32Value = Assert.IsAssignableFrom<Integer32Value>(newValue);
            Assert.Equal(expectedBits, int32Value.U32);
            Assert.Equal(expectedMask, int32Value.Mask);
        }

        [Theory]
        [InlineData("1100??00", false,
            0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11000000UL, 
            0b11111111_11111111_11111111_11111111_11111111_11111111_11111111_11110011UL)]
        [InlineData("1100??00", true, 
            0b11111111_11111111_11111111_11111111_11111111_11111111_11111111_11000000UL, 
            0b11111111_11111111_11111111_11111111_11111111_11111111_11111111_11110011UL)]
        [InlineData("?100??00", false, 
            0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01000000UL, 
            0b11111111_11111111_11111111_11111111_11111111_11111111_11111111_01110011UL)]
        [InlineData("?100??00", true,
            0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01000000UL, 
            0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01110011UL)]
        public void Extend8BitsTo64Bits(string input, bool signExtend, ulong expectedBits, ulong expectedMask)
        {
            var value = new IntegerNValue(input);
            var newValue = value.Extend(64, signExtend);
                
            var int64Value = Assert.IsAssignableFrom<Integer64Value>(newValue);
            Assert.Equal(expectedBits, int64Value.U64);
            Assert.Equal(expectedMask, int64Value.Mask);
        }

        [Theory]
        [InlineData("1100110011110000", 0b11110000, 0b11111111)]
        [InlineData("??00110011??0000", 0b11000000, 0b11001111)]
        public void TruncateTo8Bits(string input, byte expectedBits, byte expectedMask)
        {
            var value = new IntegerNValue(input);
            var newValue = value.Truncate(8);
            
            var int8Value = Assert.IsAssignableFrom<Integer8Value>(newValue);
            Assert.Equal(expectedBits, int8Value.U8);
            Assert.Equal(expectedMask, int8Value.Mask);
        }

        [Theory]
        [InlineData("01010101", "01010101", True)]
        [InlineData("01010101", "10101010", False)]
        [InlineData("010?0101", "01010101", Unknown)]
        [InlineData("010?0111", "01010101", False)]
        public void IsEqualTo(string a, string b, TrileanValue expected)
        {
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);
            
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
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

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
            var value1 = new IntegerNValue(a);
            var value2 = new IntegerNValue(b);

            Assert.Equal(expected, value1.IsGreaterThan(value2, signed));
        }
    }
}