using System;
using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Float64Test
    {
        [Fact]
        public void PersistentBits()
        {
            const double testValue = 0.123D;
            
            var value = new Float64Value(testValue);

            Span<byte> buffer = stackalloc byte[sizeof(double)];
            Span<byte> mask = stackalloc byte[sizeof(double)];
            value.GetBits(buffer);
            value.GetMask(mask);
            
            value.SetBits(buffer, mask);
            
            Assert.Equal(testValue, value.F64);
        }
    }
}