using System.Buffers;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ReferenceType
{
    public class MemoryPointerValueTest
    {
        [Fact]
        public void ReadInteger16()
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(2);
            using var bitmaskOwner = MemoryPool<byte>.Shared.Rent(2);

            var ptrValue = new MemoryPointerValue(memoryOwner.Memory, bitmaskOwner.Memory, true);
            
            var value = new Integer16Value("00001111????0000");
            ptrValue.WriteInteger16(0, value);
            Assert.Equal(value, ptrValue.ReadInteger16(0));
        }

        [Fact]
        public void WriteInteger16ShouldBeLittleEndian()
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(2);
            using var bitmaskOwner = MemoryPool<byte>.Shared.Rent(2);

            var ptrValue = new MemoryPointerValue(memoryOwner.Memory, bitmaskOwner.Memory, true);
            
            var value = new Integer16Value("00001111????0000");
            ptrValue.WriteInteger16(0, value);
            
            Assert.Equal(new Integer8Value("????0000"), ptrValue.ReadInteger8(0));
            Assert.Equal(new Integer8Value("00001111"), ptrValue.ReadInteger8(1));
        }
        
        [Fact]
        public void ReadInteger32()
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(4);
            using var bitmaskOwner = MemoryPool<byte>.Shared.Rent(4);

            var ptrValue = new MemoryPointerValue(memoryOwner.Memory, bitmaskOwner.Memory, true);
            
            var value = new Integer32Value("00001111????0011001100??00??0101");
            ptrValue.WriteInteger32(0, value);
            Assert.Equal(value, ptrValue.ReadInteger32(0));
        }

        [Fact]
        public void WriteInteger32ShouldBeLittleEndian()
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(4);
            using var bitmaskOwner = MemoryPool<byte>.Shared.Rent(4);

            var ptrValue = new MemoryPointerValue(memoryOwner.Memory, bitmaskOwner.Memory, true);

            var value = new Integer32Value("00001111" + "????0011" + "001100??" + "00??0101");
            ptrValue.WriteInteger32(0, value);
            
            Assert.Equal(new Integer8Value("00??0101"), ptrValue.ReadInteger8(0));
            Assert.Equal(new Integer8Value("001100??"), ptrValue.ReadInteger8(1));
            Assert.Equal(new Integer8Value("????0011"), ptrValue.ReadInteger8(2));
            Assert.Equal(new Integer8Value("00001111"), ptrValue.ReadInteger8(3));
        }
        
        [Fact]
        public void ReadInteger64()
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(8);
            using var bitmaskOwner = MemoryPool<byte>.Shared.Rent(8);

            var ptrValue = new MemoryPointerValue(memoryOwner.Memory, bitmaskOwner.Memory, true);
            
            var value = new Integer32Value("00001111????0011001100??00??0101");
            ptrValue.WriteInteger32(0, value);
            Assert.Equal(value, ptrValue.ReadInteger32(0));
        }

        [Fact]
        public void WriteInteger64ShouldBeLittleEndian()
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(8);
            using var bitmaskOwner = MemoryPool<byte>.Shared.Rent(8);

            var ptrValue = new MemoryPointerValue(memoryOwner.Memory, bitmaskOwner.Memory, true);

            var value = new Integer64Value("00000000" + "11111111" + "????????" + "00001111" +
                                           "????0000" + "01010101" + "0?0?0?0?" + "01010?0?");
            ptrValue.WriteInteger64(0, value);
            
            Assert.Equal(new Integer8Value("01010?0?"), ptrValue.ReadInteger8(0));
            Assert.Equal(new Integer8Value("0?0?0?0?"), ptrValue.ReadInteger8(1));
            Assert.Equal(new Integer8Value("01010101"), ptrValue.ReadInteger8(2));
            Assert.Equal(new Integer8Value("????0000"), ptrValue.ReadInteger8(3));
            Assert.Equal(new Integer8Value("00001111"), ptrValue.ReadInteger8(4));
            Assert.Equal(new Integer8Value("????????"), ptrValue.ReadInteger8(5));
            Assert.Equal(new Integer8Value("11111111"), ptrValue.ReadInteger8(6));
            Assert.Equal(new Integer8Value("00000000"), ptrValue.ReadInteger8(7));
        }

        [Fact]
        public void ReadWriteFloat32()
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(4);
            using var bitmaskOwner = MemoryPool<byte>.Shared.Rent(4);

            var ptrValue = new MemoryPointerValue(memoryOwner.Memory, bitmaskOwner.Memory, true);

            var value = new Float32Value(0.12345678f);
            ptrValue.WriteFloat32(0, value);
            Assert.Equal(value.F32, ptrValue.ReadFloat32(0).F32);
        }

        [Fact]
        public void ReadWriteFloat64()
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(8);
            using var bitmaskOwner = MemoryPool<byte>.Shared.Rent(8);

            var ptrValue = new MemoryPointerValue(memoryOwner.Memory, bitmaskOwner.Memory, true);

            var value = new Float64Value(0.12345678d);
            ptrValue.WriteFloat64(0, value);
            Assert.Equal(value.F64, ptrValue.ReadFloat64(0).F64);
        }
    }
}