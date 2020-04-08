using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Integer32ValueTests
    {
        [Fact]
        public void KnownValueSignUnsignedSame()
        {
            var value = new Integer32Value(0x12345678);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer32Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x12345678u, value.U32);
            Assert.Equal(0x12345678, value.I32);
        }
        
        [Fact]
        public void KnownValueSignUnsignedDifferent()
        {
            var value = new Integer32Value(0x8000_0000);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer32Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x8000_0000, value.U32);
            Assert.Equal(-0x8000_0000, value.I32);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer32Value(
                0b00110011_00110011_00001111_11110000, 
                0b00110011_11001100_00110011_11001100);
            
            Assert.False(value.IsKnown);
            Assert.Equal(0b00110011_00000000_00000011_11000000u, value.U32);
        }

        [Theory]
        [InlineData(3, null)]
        [InlineData(5, true)]
        [InlineData(6, false)]
        public void ReadBit(int index, bool? expected)
        {
            var value = new Integer32Value(
                0b00000000_00100000,
                0b11111111_11110000);
            Assert.Equal(expected, value.GetBit(index));
        }

        [Theory]
        [InlineData(31, null)]
        [InlineData(31, true)]
        [InlineData(31, false)]
        public void SetBit(int index, bool? expected)
        {
            var value = new Integer32Value(0);
            value.SetBit(index, expected);
            Assert.Equal(expected, value.GetBit(index));
        }
    }

}