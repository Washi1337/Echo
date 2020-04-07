using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Integer32Tests
    {
        [Fact]
        public void KnownValueSignUnsignedSame()
        {
            var value = new Integer32(0x12345678);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer32.FullyKnownMask, value.Mask);
            Assert.Equal(0x12345678u, value.U32);
            Assert.Equal(0x12345678, value.I32);
        }
        
        [Fact]
        public void KnownValueSignUnsignedDifferent()
        {
            var value = new Integer32(0x8000_0000);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer32.FullyKnownMask, value.Mask);
            Assert.Equal(0x8000_0000, value.U32);
            Assert.Equal(-0x8000_0000, value.I32);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer32(
                0b00110011_00110011_00001111_11110000, 
                0b00110011_11001100_00110011_11001100);
            
            Assert.False(value.IsKnown);
            Assert.Equal(0b00110011_00000000_00000011_11000000u, value.U32);
        }
    }

}