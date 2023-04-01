using Echo.Memory;
using Xunit;

namespace Echo.Tests.Memory
{
    public class BitVectorPoolTest
    {
        private readonly BitVectorPool _pool = new();

        [Theory]
        [InlineData(8)]
        [InlineData(16)]
        public void RentShouldReturnVectorWithAppropriateSize(int size)
        {
            var vector = _pool.Rent(size, false);
            Assert.Equal(size, vector.Count);
        }

        [Fact]
        public void RentShouldReturnDifferentInstancesIfNoVectorFree()
        {
            var vector1 = _pool.Rent(8, false);
            var vector2 = _pool.Rent(8, false);
            
            Assert.NotSame(vector1, vector2);
        }

        [Fact]
        public void ReturnShouldBeTrueIfVectorIsFromPool()
        {
            var vector = _pool.Rent(16, false);
            Assert.True(_pool.Return(vector));
        }

        [Fact]
        public void ReturnShouldBeFalseIfVectorIsNotFromPool()
        {
            var vector = new BitVector(8, false);
            Assert.False(_pool.Return(vector));
        }

        [Fact]
        public void RentShouldProvideReturnedVectorsIfSameSize()
        {
            var vector1 = _pool.Rent(8, false);
            var vector2 = _pool.Rent(8, false);
            _pool.Return(vector2);

            var vector3 = _pool.Rent(8, false);
            Assert.Same(vector2, vector3);
            Assert.True(_pool.Return(vector2));

            var vector4 = _pool.Rent(16, false);
            Assert.NotSame(vector2, vector4);
        }
    }
}