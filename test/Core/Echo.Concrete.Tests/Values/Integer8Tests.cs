using Echo.Concrete.Values;
using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values
{
    public class Integer8Tests
    {
        [Fact]
        public void KnownValue()
        {
            var value = new Integer8(0xFF);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer8.FullyKnownMask, value.Mask);
            Assert.Equal(0xFF, value.U8);
            Assert.Equal(-1, value.I8);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer8(0b00110011, 0b11101101);
            Assert.False(value.IsKnown);
            Assert.Equal(0b00100001, value.U8);
        }
    }
}