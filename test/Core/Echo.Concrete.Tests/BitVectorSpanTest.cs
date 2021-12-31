using System;
using Xunit;

namespace Echo.Concrete.Tests
{
    public class BitVectorSpanTest
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
    }
}