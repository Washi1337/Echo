using Echo.Memory;
using Xunit;

namespace Echo.Tests.Memory
{
    public class BitVectorTest
    {
        [Theory]
        [InlineData(false, "00000000", "0000000000000000")]
        [InlineData(true, "00000000","0000000000000000")]
        [InlineData(false, "10000000", "0000000010000000")]
        [InlineData(true, "10000000","1111111110000000")]
        [InlineData(false, "?0000000", "00000000?0000000")]
        [InlineData(true, "?0000000","?????????0000000")]
        public void ResizeToLargerVector(bool signExtend, string startValue, string expectedValue)
        {
            var vector = BitVector.ParseBinary(startValue);
            var newVector = vector.Resize(16, signExtend);
            Assert.Equal(expectedValue, newVector.AsSpan().ToBitString());
        }
    }
}