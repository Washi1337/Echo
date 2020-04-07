using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Integer64Tests
    {
        [Fact]
        public void KnownValueSignUnsignedSame()
        {
            var value = new Integer64Value(0x12345678_9ABCDEF0);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer64Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x12345678_9ABCDEF0u, value.U64);
            Assert.Equal(0x12345678_9ABCDEF0, value.I64);
        }
        
        [Fact]
        public void KnownValueSignUnsignedDifferent()
        {
            var value = new Integer64Value(0x80000000_00000000);

            Assert.True(value.IsKnown);
            Assert.Equal(Integer64Value.FullyKnownMask, value.Mask);
            Assert.Equal(0x80000000_00000000, value.U64);
            Assert.Equal(-0x80000000_00000000, value.I64);
        }

        [Fact]
        public void PartiallyUnknownValue()
        {
            var value = new Integer64Value(
                0b111111111_11111111_11111111_11111111_11111111_11111111_11111111_1111111, 
                0b000000000_11111111_00001111_11110000_00110011_11001100_01010101_1010101);
            
            Assert.False(value.IsKnown);
            Assert.Equal(0b000000000_11111111_00001111_11110000_00110011_11001100_01010101_1010101u, value.U64);
        }
    }

}