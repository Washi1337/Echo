using System.Buffers;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ReferenceType
{
    public class MemoryBlockValueTest
    {
        [Fact]
        public void ReadInteger16()
        {
            var ptrValue = new MemoryBlockValue(2);
            
            var value = new Integer16Value("00001111????0000");
            ptrValue.WriteInteger16(0, value);
            Assert.Equal(value, ptrValue.ReadInteger16(0));
        }

        [Fact]
        public void WriteInteger16ShouldBeLittleEndian()
        {
            var ptrValue = new MemoryBlockValue(2);
            
            var value = new Integer16Value("00001111????0000");
            ptrValue.WriteInteger16(0, value);
            
            Assert.Equal(new Integer8Value("????0000"), ptrValue.ReadInteger8(0));
            Assert.Equal(new Integer8Value("00001111"), ptrValue.ReadInteger8(1));
        }
        
        [Fact]
        public void ReadInteger32()
        {
            var ptrValue = new MemoryBlockValue(4);
            
            var value = new Integer32Value("00001111????0011001100??00??0101");
            ptrValue.WriteInteger32(0, value);
            Assert.Equal(value, ptrValue.ReadInteger32(0));
        }

        [Fact]
        public void WriteInteger32ShouldBeLittleEndian()
        {
            var ptrValue = new MemoryBlockValue(4);

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
            var ptrValue = new MemoryBlockValue(8);
            
            var value = new Integer32Value("00001111????0011001100??00??0101");
            ptrValue.WriteInteger32(0, value);
            Assert.Equal(value, ptrValue.ReadInteger32(0));
        }

        [Fact]
        public void WriteInteger64ShouldBeLittleEndian()
        {
            var ptrValue = new MemoryBlockValue(8);

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
            var ptrValue = new MemoryBlockValue(4);

            var value = new Float32Value(0.12345678f);
            ptrValue.WriteFloat32(0, value);
            Assert.Equal(value.F32, ptrValue.ReadFloat32(0).F32);
        }

        [Fact]
        public void ReadWriteFloat64()
        {
            var ptrValue = new MemoryBlockValue(8);

            var value = new Float64Value(0.12345678d);
            ptrValue.WriteFloat64(0, value);
            Assert.Equal(value.F64, ptrValue.ReadFloat64(0).F64);
        }
    }
}