using System;
using Xunit;

namespace Echo.Concrete.Tests
{
    public partial class BitVectorSpanTest
    {
        [Theory]
        [InlineData(1f)]
        [InlineData(-1f)]
        public void Float32NegateFullyKnown(float x)
        {
            var vector = new BitVector(x).AsSpan();
            vector.FloatNegate();
            
            Assert.Equal(-x, vector.F32);
        }
        
        [Theory]
        [InlineData(1D)]
        [InlineData(-1D)]
        public void Float64NegateFullyKnown(Double x)
        {
            var vector = new BitVector(x).AsSpan();
            vector.FloatNegate();
            
            Assert.Equal(-x, vector.F64);
        }
        
        [Theory]
        [InlineData(1f, 2f, 3f)]
        public void Float32Add(float a, float b, float expected)
        {
            var value1 = new BitVector(a).AsSpan();
            var value2 = new BitVector(b).AsSpan();
            value1.FloatAdd(value2);
            
            Assert.Equal(expected, value1.F32);
        }
        
        [Theory]
        [InlineData(1f, 2f, -1f)]
        public void Float32Subtract(float a, float b, float expected)
        {
            var value1 = new BitVector(a).AsSpan();
            var value2 = new BitVector(b).AsSpan();
            value1.FloatSubtract(value2);
            
            Assert.Equal(expected, value1.F32);
        }
        
        [Theory]
        [InlineData(3f, 2f, 6f)]
        public void Float32Multiply(float a, float b, float expected)
        {
            var value1 = new BitVector(a).AsSpan();
            var value2 = new BitVector(b).AsSpan();
            value1.FloatMultiply(value2);
            
            Assert.Equal(expected, value1.F32);
        }
        
        [Theory]
        [InlineData(10f, 2f, 5f)]
        public void Float32Divide(float a, float b, float expected)
        {
            var value1 = new BitVector(a).AsSpan();
            var value2 = new BitVector(b).AsSpan();
            value1.FloatDivide(value2);
            
            Assert.Equal(expected, value1.F32);
        }   
    }
}