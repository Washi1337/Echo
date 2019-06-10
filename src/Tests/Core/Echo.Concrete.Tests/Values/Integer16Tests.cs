using Echo.Concrete.Values;
using Xunit;

namespace Echo.Concrete.Tests.Values
{
    public class Integer16Tests
    {
        [Fact]
        public void KnownValue()
        {
            var value = new Integer16(0xFFFF);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer16.FullyKnownMask, value.Mask);
            Assert.Equal(0xFFFF, value.U16);
            Assert.Equal(-1, value.I16);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer16(0b00110011_00110011, 0b00110011_11001100);
            Assert.False(value.IsKnown);
            Assert.Equal(0b00110011_00000000, value.U16);
        }
    }

}