using System;
using System.Text;
using Echo.Memory;
using Xunit;

namespace Echo.Tests.Memory
{
    public class BasicMemorySpaceTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData(0x0040_0000)]
        public void ReadTest(long baseAddress)
        {
            byte[] actualData = Encoding.ASCII.GetBytes("Hello, world!");
            
            var backBuffer = new BitVector(actualData);
            var space = new BasicMemorySpace(backBuffer);
            space.Rebase(baseAddress);

            Span<byte> bits = stackalloc byte[5];
            Span<byte> known = stackalloc byte[5];
            var buffer = new Echo.Memory.BitVectorSpan(bits, known);

            space.Read(baseAddress + 1, buffer);

            Assert.Equal(Convert.ToHexString(actualData[1..6]), buffer.ToHexString());
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(0x0040_0000)]
        public void WriteTest(long baseAddress)
        {            
            byte[] actualData = Encoding.ASCII.GetBytes("Hello, world!");

            var backBuffer = new BitVector(100 * 8, false);
            var space = new BasicMemorySpace(backBuffer);
            space.Rebase(baseAddress);

            Span<byte> bits = stackalloc byte[5];
            Span<byte> known = stackalloc byte[5];
            var readBuffer = new Echo.Memory.BitVectorSpan(bits, known);
            
            space.Read(baseAddress + 1, readBuffer);

            Assert.Equal("??????????", readBuffer.ToHexString());

            var writeBuffer = new BitVector(actualData);
            space.Write(baseAddress + 1, writeBuffer.AsSpan());
            
            space.Read(baseAddress + 1, readBuffer);
            
            Assert.Equal(Convert.ToHexString(actualData[..5]), readBuffer.ToHexString());
        }
    }
}