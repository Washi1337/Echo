using System;
using Echo.Core;
using Xunit;
using static Echo.Core.TrileanValue;

namespace Echo.Concrete.Tests
{
    public partial class BitVectorSpanTest
    {
        [Fact]
        public void AllZeroes()
        {
            var vector = new BitVector(16, true);
            Assert.Equal("0000000000000000", vector.AsSpan().ToBitString());
            Assert.Equal("0000", vector.AsSpan().ToHexString());
        }
        
        [Fact]
        public void AllOnes()
        {
            var vector = new BitVector(16, true);
            vector.Bits.AsSpan().Fill(0xFF);
            Assert.Equal("1111111111111111", vector.AsSpan().ToBitString());
            Assert.Equal("FFFF", vector.AsSpan().ToHexString());
        }
        
        [Fact]
        public void AllUnknown()
        {
            var vector = new BitVector(16, false);
            Assert.Equal("????????????????", vector.AsSpan().ToBitString());
            Assert.Equal("????", vector.AsSpan().ToHexString());
        }
        
        [Fact]
        public void MixKnownUnknown()
        {
            var vector = new BitVector(16, true);
            vector.Bits[0] = 0xF0;
            vector.Bits[1] = 0xF0;
            vector.KnownMask[1] = 0b00001111;
            Assert.Equal("????000011110000", vector.AsSpan().ToBitString());
            Assert.Equal("F0?0", vector.AsSpan().ToHexString());
        }

        [Fact]
        public void SliceAtIndex()
        {
            var vector = new BitVector(new byte[]
            {
                0b10101010,
                0b11001100,
                0b11110000,
                0b11111111,
                0b00000000,
            });
            
            Assert.Equal("00000000111111111111000011001100", vector.AsSpan(8).ToBitString());
        }

        [Fact]
        public void SliceAtIndexWithLength()
        {
            var vector = new BitVector(new byte[]
            {
                0b10101010,
                0b11001100,
                0b11110000,
                0b11111111,
                0b00000000,
            });
            
            Assert.Equal("10101010", vector.AsSpan(0, 8).ToBitString());
        }


        [Fact]
        public void HexStringTest()
        {
            var vector = new BitVector(new byte[] {0x12, 0x34, 0x56, 0x78});
            Assert.Equal("12345678", vector.AsSpan().ToHexString());
        }

        [Theory]
        [InlineData(0, False, "00000000")]
        [InlineData(0, True, "00000001")]
        [InlineData(0, Unknown, "0000000?")]
        [InlineData(3, False, "00000000")]
        [InlineData(3, True, "00001000")]
        [InlineData(3, Unknown, "0000?000")]
        [InlineData(4, False, "00000000")]
        [InlineData(4, True, "00010000")]
        [InlineData(4, Unknown, "000?0000")]
        public void SetSingleBit(int index, TrileanValue value, string expected)
        {
            var vector = new BitVector(8, true).AsSpan();
            vector[index] = value;
            Assert.Equal(expected, vector.ToBitString());
        }

        [Fact]
        public void SetMultipleBits()
        {
            var vector = new BitVector(8, true).AsSpan();
            
            vector[0] = Trilean.False;
            vector[1] = Trilean.True;
            vector[2] = Trilean.Unknown;
            vector[3] = Trilean.False;
            vector[4] = Trilean.True;
            vector[5] = Trilean.Unknown;
            vector[6] = Trilean.False;
            vector[7] = Trilean.True;
            
            Assert.Equal("10?10?10", vector.ToBitString());
        }

        [Theory]
        [InlineData("00000000")]
        [InlineData("11111111")]
        [InlineData("????????")]
        [InlineData("10?10?10")]
        public void Parse(string binaryString)
        {
            var vector = BitVector.ParseBinary(binaryString);
            Assert.Equal(binaryString, vector.AsSpan().ToBitString());
        }

        [Theory]
        [InlineData("0000000000000000", True)]
        [InlineData("0000000000000001", False)]
        [InlineData("????????????????", Unknown)]
        [InlineData("000000000000000?", Unknown)]
        [InlineData("?000000000000000", Unknown)]
        [InlineData("1???????????????", False)]
        [InlineData("???????????????1", False)]
        [InlineData("000010000000000?", False)]
        [InlineData("000000000000100?", False)]
        public void IsZero(string binaryString, TrileanValue expected)
        {
            var vector = BitVector.ParseBinary(binaryString).AsSpan();
            Assert.Equal(expected, vector.IsZero);
        }

        [Theory]
        [InlineData("01010101", "01010101", True)]
        [InlineData("01010101", "10101010", False)]
        [InlineData("010?0101", "01010101", Unknown)]
        [InlineData("010?0111", "01010101", False)]
        public void IsEqualTo(string a, string b, TrileanValue expected)
        {
            var value1 = BitVector.ParseBinary(a).AsSpan();
            var value2 = BitVector.ParseBinary(b).AsSpan();

            Assert.Equal(expected, value1.IsEqualTo(value2));
        }
    }
}