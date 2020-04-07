using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Integer16Tests
    {
        [Fact]
        public void KnownValueSignUnsignedSame()
        {
            var value = new Integer16(0x1234);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer16.FullyKnownMask, value.Mask);
            Assert.Equal(0x1234, value.U16);
            Assert.Equal(0x1234, value.I16);
        }
        
        [Fact]
        public void KnownValueSignUnsignedDifferent()
        {
            var value = new Integer16(0x8000);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer16.FullyKnownMask, value.Mask);
            Assert.Equal(0x8000, value.U16);
            Assert.Equal(-0x8000, value.I16);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer16(
                0b00110011_00110011, 
                0b00110011_11001100);
            
            Assert.False(value.IsKnown);
            Assert.Equal(0b00110011_00000000, value.U16);
        }
    }

}