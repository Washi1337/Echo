using System;
using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ValueType
{
    public class Float32Test
    {
        [Fact]
        public void PersistentBits()
        {
            const float testValue = 0.123f;
            
            var value = new Float32Value(testValue);

            Span<byte> buffer = stackalloc byte[sizeof(float)];
            Span<byte> mask = stackalloc byte[sizeof(float)];
            value.GetBits(buffer);
            value.GetMask(mask);
            
            value.SetBits(buffer, mask);
            
            Assert.Equal(testValue, value.F32);
        }
    }
}