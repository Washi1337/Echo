using System;
using System.Linq;
using System.Text;
using Echo.Memory;
using Echo.Code;
using Xunit;

namespace Echo.Tests.Memory
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
            writeBuffer.Write(Encoding.ASCII.GetBytes("Hello, World!"));
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

        [Fact]
        public void ReadFromUnmappedShouldThrow()
        {
            var memory = new VirtualMemory();

            memory.Map(0x1000_0000, new BasicMemorySpace(new byte[] {1, 2, 3, 4, 5, 6, 7, 8}));

            Assert.Throws<AccessViolationException>(() => memory.Read(0x2000_0000, new BitVector(32, false)));
        }

        [Fact]
        public void WriteToUnmappedShouldThrow()
        {
            var memory = new VirtualMemory();

            memory.Map(0x1000_0000, new BasicMemorySpace(new byte[] {1, 2, 3, 4, 5, 6, 7, 8}));

            Assert.Throws<AccessViolationException>(() => memory.Write(0x2000_0000, new BitVector(32, false)));
            Assert.Throws<AccessViolationException>(() => memory.Write(0x2000_0000, new byte[4]));
        }
        
        [Fact]
        public void ReadFromSingleSpace()
        {
            var memory = new VirtualMemory();

            memory.Map(0x1000_0000, new BasicMemorySpace(new byte[] {1, 2, 3, 4, 5, 6, 7, 8}));
            memory.Map(0x2000_0000, new BasicMemorySpace(new byte[] {11, 22, 33, 44, 55, 66, 77, 88}));

            var buffer = new BitVector(32, false);

            memory.Read(0x1000_0002, buffer);
            Assert.Equal(new byte[] {3, 4, 5, 6}, buffer.Bits);
            
            memory.Read(0x2000_0002, buffer);
            Assert.Equal(new byte[] {33, 44, 55, 66}, buffer.Bits);
        }

        [Fact]
        public void WriteToSingleSpace()
        {
            var memory = new VirtualMemory();

            var space1 = new BasicMemorySpace(new byte[8]);
            var space2 = new BasicMemorySpace(new byte[8]);
            memory.Map(0x1000_0000, space1);
            memory.Map(0x2000_0000, space2);

            memory.Write(0x1000_0002, new BitVector(new byte[] {1, 2, 3, 4}));
            Assert.Equal(new byte[] {0, 0, 1, 2, 3, 4, 0, 0}, space1.BackBuffer.Bits);
            Assert.Equal(new byte[] {0, 0, 0, 0, 0, 0, 0, 0}, space2.BackBuffer.Bits);
            
            memory.Write(0x2000_0002, new BitVector(new byte[] {11, 22, 33, 44}));
            Assert.Equal(new byte[] {0, 0, 1, 2, 3, 4, 0, 0}, space1.BackBuffer.Bits);
            Assert.Equal(new byte[] {0, 0, 11, 22, 33, 44, 0, 0}, space2.BackBuffer.Bits);
            
            memory.Write(0x1000_0004, new byte[] {11, 22, 33, 44});
            Assert.Equal(new byte[] {0, 0, 1, 2, 11, 22, 33, 44}, space1.BackBuffer.Bits);
            Assert.Equal(new byte[] {0, 0, 11, 22, 33, 44, 0, 0}, space2.BackBuffer.Bits);
            
            memory.Write(0x2000_0004, new byte[] {1, 2, 3, 4});
            Assert.Equal(new byte[] {0, 0, 1, 2, 11, 22, 33, 44}, space1.BackBuffer.Bits);
            Assert.Equal(new byte[] {0, 0, 11, 22, 1, 2, 3, 4}, space2.BackBuffer.Bits);
        }

        [Fact]
        public void ReadFromMultipleSpaces()
        {
            var memory = new VirtualMemory();

            memory.Map(0x1000_0000, new BasicMemorySpace(new byte[] {1, 2, 3, 4, 5, 6, 7, 8}));
            memory.Map(0x1000_0008, new BasicMemorySpace(new byte[] {9, 10, 11, 12, 13, 14, 15, 16}));
            memory.Map(0x1000_0010, new BasicMemorySpace(new byte[] {17, 18, 19, 20, 21, 22, 23, 24}));

            var buffer = new BitVector(16 * 8, false);

            memory.Read(0x1000_0004, buffer);

            Assert.Equal(new byte[]
            {
                5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
            }, buffer.Bits);
        }

        [Fact]
        public void WriteToMultipleSpaces()
        {
            var memory = new VirtualMemory();

            var space1 = new BasicMemorySpace(new byte[8]);
            var space2 = new BasicMemorySpace(new byte[8]);
            var space3 = new BasicMemorySpace(new byte[8]);

            memory.Map(0x1000_0000, space1);
            memory.Map(0x1000_0008, space2);
            memory.Map(0x1000_0010, space3);

            var buffer = new BitVector(new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
            });

            memory.Write(0x1000_0004, buffer);

            Assert.Equal(new byte[] {0, 0, 0, 0, 1, 2, 3, 4}, space1.BackBuffer.Bits);
            Assert.Equal(new byte[] {5, 6, 7, 8, 9, 10, 11, 12}, space2.BackBuffer.Bits);
            Assert.Equal(new byte[] {13, 14, 15, 16, 0, 0, 0, 0}, space3.BackBuffer.Bits);

            byte[] buffer2 = {
                16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1
            };

            memory.Write(0x1000_0004, buffer2);

            Assert.Equal(new byte[] {0, 0, 0, 0, 16, 15, 14, 13}, space1.BackBuffer.Bits);
            Assert.Equal(new byte[] {12, 11, 10, 9, 8, 7, 6, 5}, space2.BackBuffer.Bits);
            Assert.Equal(new byte[] {4, 3, 2, 1, 0, 0, 0, 0}, space3.BackBuffer.Bits);
        }
    }
}