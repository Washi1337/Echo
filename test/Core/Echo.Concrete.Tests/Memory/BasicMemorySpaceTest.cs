using System;
using System.Text;
using Echo.Concrete.Memory;
using Xunit;

namespace Echo.Concrete.Tests.Memory
{
    public class BasicMemorySpaceTest
    {
        [Fact]
        public void ReadTest()
        {
            byte[] actualData = Encoding.ASCII.GetBytes("Hello, world!");
            
            var backBuffer = new BitVector(actualData);
            var space = new BasicMemorySpace(backBuffer);

            Span<byte> bits = stackalloc byte[5];
            Span<byte> known = stackalloc byte[5];
            var buffer = new BitVectorSpan(bits, known);
            
            space.Read(1, buffer);

            Assert.Equal(Convert.ToHexString(actualData[1..6]), buffer.ToHexString());
        }
        
        [Fact]
        public void WriteTest()
        {            
            byte[] actualData = Encoding.ASCII.GetBytes("Hello, world!");

            var backBuffer = new BitVector(100 * 8, false);
            var space = new BasicMemorySpace(backBuffer);

            Span<byte> bits = stackalloc byte[5];
            Span<byte> known = stackalloc byte[5];
            var readBuffer = new BitVectorSpan(bits, known);
            
            space.Read(1, readBuffer);

            Assert.Equal("??????????", readBuffer.ToHexString());

            var writeBuffer = new BitVector(actualData);
            space.Write(1, writeBuffer.AsSpan());
            
            space.Read(1, readBuffer);
            
            Assert.Equal(Convert.ToHexString(actualData[..5]), readBuffer.ToHexString());
        }
    }
}