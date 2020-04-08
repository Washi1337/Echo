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
    }

}