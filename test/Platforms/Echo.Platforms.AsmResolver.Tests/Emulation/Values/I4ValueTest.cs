using Echo.Platforms.AsmResolver.Emulation.Values;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Values
{
    public class I4ValueTest
    {
        [Theory]
        [InlineData(1, false, 1, false)]
        [InlineData(1, true, 1, false)]
        [InlineData(0x80, false, -0x80, true)]
        [InlineData(0x80, true, null, true)]
        [InlineData(-0x80, true, null, true)]
        public void ConvertToI1(int value, bool unsigned, int? expectedValue, bool expectedToOverflow)
        {
            var i4Value = new I4Value(value);
            var i1Value = i4Value.ConvertToI1(unsigned, out bool overflowed);

            if (expectedValue.HasValue)
                Assert.Equal(expectedValue.Value, i1Value.I32);

            Assert.Equal(expectedToOverflow, overflowed);
        }

        [Theory]
        [InlineData(1, false, 1u, false)]
        [InlineData(1, true, 1u, false)]
        [InlineData(0x80, false, 0x80u, false)]
        [InlineData(0x80, true, 0x80u, false)]
        [InlineData(-0x80, false, null, true)]
        [InlineData(-0x80, true, null, true)]
        public void ConvertToU1(int value, bool unsigned, uint? expectedValue, bool expectedToOverflow)
        {
            var i4Value = new I4Value(value);
            var u1Value = i4Value.ConvertToU1(unsigned, out bool overflowed);
            
            if (expectedValue.HasValue)
                Assert.Equal(expectedValue.Value, u1Value.U32);
            Assert.Equal(expectedToOverflow, overflowed);
        }
        
        [Theory]
        [InlineData(1, false, 1, false)]
        [InlineData(1, true, 1, false)]
        [InlineData(0x8000, false, -0x8000, true)]
        [InlineData(0x8000, true, null, true)]
        [InlineData(-0x8000, true, null, true)]
        public void ConvertToI2(int value, bool unsigned, int? expectedValue, bool expectedToOverflow)
        {
            var i4Value = new I4Value(value);
            var i2Value = i4Value.ConvertToI2(unsigned, out bool overflowed);

            if (expectedValue.HasValue)
                Assert.Equal(expectedValue.Value, i2Value.I32);

            Assert.Equal(expectedToOverflow, overflowed);
        }

        [Theory]
        [InlineData(1, false, 1u, false)]
        [InlineData(1, true, 1u, false)]
        [InlineData(0x8000, false, 0x8000u, false)]
        [InlineData(0x8000, true, 0x8000u, false)]
        [InlineData(-0x8000, false, null, true)]
        [InlineData(-0x8000, true, null, true)]
        public void ConvertToU2(int value, bool unsigned, uint? expectedValue, bool expectedToOverflow)
        {
            var i4Value = new I4Value(value);
            var u2Value = i4Value.ConvertToU2(unsigned, out bool overflowed);
            
            if (expectedValue.HasValue)
                Assert.Equal(expectedValue.Value, u2Value.U32);
            Assert.Equal(expectedToOverflow, overflowed);
        }
        
        [Fact]
        public void InterpretAsI4()
        {
            var value = new I4Value(1234);
            Assert.Equal(value, value.InterpretAsI4());
        }
        
        [Theory]
        [InlineData(1, false, 1u, false)]
        [InlineData(1, true, 1u, false)]
        [InlineData(int.MinValue, false, 0x80000000u, true)]
        [InlineData(int.MinValue, true, 0x80000000u, false)]
        public void ConvertToU4(int value, bool unsigned, uint? expectedValue, bool expectedToOverflow)
        {
            var i4Value = new I4Value(value);
            var u4Value = i4Value.ConvertToU4(unsigned, out bool overflowed);
            
            if (expectedValue.HasValue)
                Assert.Equal(expectedValue.Value, u4Value.U32);
            Assert.Equal(expectedToOverflow, overflowed);
        }
        
        [Theory]
        [InlineData(1, false, 1u, false)]
        [InlineData(1, true, 1u, false)]
        [InlineData(int.MinValue, false, int.MinValue, false)]
        [InlineData(int.MinValue, true, int.MinValue, false)]
        public void ConvertToI8(int value, bool unsigned, long? expectedValue, bool expectedToOverflow)
        {
            var i4Value = new I4Value(value);
            var i8Value = i4Value.ConvertToI8(unsigned, out bool overflowed);
            
            if (expectedValue.HasValue)
                Assert.Equal(expectedValue.Value, i8Value.I64);
            Assert.Equal(expectedToOverflow, overflowed);
        }
        
        [Theory]
        [InlineData(1, false, 1u, false)]
        [InlineData(1, true, 1u, false)]
        [InlineData(int.MinValue, false, 0x80000000, true)]
        [InlineData(int.MinValue, true, 0x80000000, false)]
        public void ConvertToU8(int value, bool unsigned, ulong? expectedValue, bool expectedToOverflow)
        {
            var i4Value = new I4Value(value);
            var u8Value = i4Value.ConvertToU8(unsigned, out bool overflowed);
            
            if (expectedValue.HasValue)
                Assert.Equal(expectedValue.Value, u8Value.U64);
            Assert.Equal(expectedToOverflow, overflowed);
        }
    }
}