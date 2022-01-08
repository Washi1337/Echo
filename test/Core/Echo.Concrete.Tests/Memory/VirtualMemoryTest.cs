using System;
using System.Text;
using Echo.Concrete.Memory;
using Echo.Core.Code;
using Xunit;

namespace Echo.Concrete.Tests.Memory
{
    public class VirtualMemoryTest
    {
        [Fact]
        public void NoMapping()
        {
            var memory = new VirtualMemory();
            
            Assert.Throws<AccessViolationException>(() =>
            {
                var vector = new BitVector(10 * 8, false);
                memory.Read(0, vector.AsSpan());
            });
        }
        
        [Fact]
        public void MapOneMemorySpace()
        {
            var memory = new VirtualMemory();

            var block = new BasicMemorySpace(1024, true);
            memory.Map(0x00400_0000, block);

            var readBuffer = new BitVector(13 * 8, false).AsSpan();
            memory.Read(0x00400_0000, readBuffer);
            Assert.Equal("00000000000000000000000000", readBuffer.ToHexString());
            
            var writeBuffer = new BitVector(13 * 8, false).AsSpan();
            writeBuffer.WriteBytes(0, Encoding.ASCII.GetBytes("Hello, World!"));
            memory.Write(0x00400_0000, writeBuffer);
            
            memory.Read(0x00400_0000, readBuffer);
            Assert.Equal("48656C6C6F2C20576F726C6421", readBuffer.ToHexString());
        }

        [Fact]
        public void MapMultipleMemorySpaces()
        {
            var memory = new VirtualMemory();

            byte[] block1Data = new byte[1024];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("Hello, earth!"), 0, block1Data, 0, 13);

            byte[] block2Data = new byte[1024];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("Hello, mars!!"), 0, block2Data, 0, 13);

            var block1 = new BasicMemorySpace(block1Data);
            memory.Map(0x00400_0000, block1);
            var block2 = new BasicMemorySpace(block2Data);
            memory.Map(0x00600_0000, block2);

            var readBuffer = new BitVector(6 * 8, false).AsSpan();
            memory.Read(0x00400_0007, readBuffer);
            Assert.Equal("656172746821", readBuffer.ToHexString());

            memory.Read(0x00600_0007, readBuffer);
            Assert.Equal("6D6172732121", readBuffer.ToHexString());

            Assert.Throws<AccessViolationException>(() =>
            {
                var buffer = new BitVector(6 * 8, false).AsSpan();
                memory.Read(0x0080_0000, buffer);
            });
        }

        [Fact]
        public void RebaseShouldRebaseSubSpaces()
        {
            var memory = new VirtualMemory(0x5000);

            var block1 = new BasicMemorySpace(0x1000, false);
            memory.Map(0x1000, block1);
            var block2 = new BasicMemorySpace(0x2000, false);
            memory.Map(0x3000, block2);
            
            memory.Rebase(0x0040_0000);

            Assert.Equal(new AddressRange(0x0040_0000, 0x0040_5000), memory.AddressRange);
            Assert.Equal(new AddressRange(0x0040_1000, 0x0040_2000), block1.AddressRange);
            Assert.Equal(new AddressRange(0x0040_3000, 0x0040_5000), block2.AddressRange);
        }
    }
}