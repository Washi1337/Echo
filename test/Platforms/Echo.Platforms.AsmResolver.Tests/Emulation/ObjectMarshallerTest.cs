using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class ObjectMarshallerTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly CilVirtualMachine _machine;

        public ObjectMarshallerTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            _machine = new CilVirtualMachine(fixture.MockModule, false);
        }

        [Theory]
        [InlineData(true, new byte[] {0x01})]
        [InlineData(false, new byte[] {0x00})]
        [InlineData((byte) 0x12, new byte[] {0x12})]
        [InlineData((sbyte) 0x12, new byte[] {0x12})]
        [InlineData((short) 0x1234, new byte[] {0x34, 0x12})]
        [InlineData((ushort) 0x1234, new byte[] {0x34, 0x12})]
        [InlineData((int) 0x12345678, new byte[] {0x78, 0x56, 0x34, 0x12})]
        [InlineData((uint) 0x12345678, new byte[] {0x78, 0x56, 0x34, 0x12})]
        [InlineData((long) 0x123456789abcdef0, new byte[] {0xf0, 0xde, 0xbc, 0x9a, 0x78, 0x56, 0x34, 0x12})]
        [InlineData((ulong) 0x123456789abcdef0, new byte[] {0xf0, 0xde, 0xbc, 0x9a, 0x78, 0x56, 0x34, 0x12})]
        public void SerializePrimitive(object? value, byte[] expected)
        {
            var result = _machine.ObjectMarshaller.ToBitVector(value);
            Assert.Equal(expected, result.Bits);
        }

        [Fact]
        public void SerializeNullShouldReturnNullReference()
        {
            var result = _machine.ObjectMarshaller.ToBitVector(null);
            Assert.True(result.AsSpan().IsZero.ToBoolean());
        }

        [Fact]
        public void SerializeStringShouldReturnStringReference()
        {
            const string value = "Hello, world!";
            var result = _machine.ObjectMarshaller.ToBitVector(value);
            
            var stringData = result.AsObjectHandle(_machine).ReadStringData();
            Assert.Equal(value, Encoding.Unicode.GetString(stringData.Bits));
        }

        [Fact]
        public void SerializeObjectShouldReturnProperObjectReference()
        {
            var obj = new SimpleClass
            {
                IntField = 1337,
                StringField = "Hello, world",
                SimpleClassField = new SimpleClass
                {
                    IntField = 1338,
                    StringField = "Hello, mars"
                }
            };

            var handle = _machine.ObjectMarshaller.ToBitVector(obj).AsObjectHandle(_machine);

            var type = handle.GetObjectType();
            Assert.Equal(nameof(SimpleClass), type.Name);

            var definition = type.Resolve()!;
            var intField = definition.Fields.First(f => f.Name == nameof(SimpleClass.IntField));
            var stringField = definition.Fields.First(f => f.Name == nameof(SimpleClass.StringField));
            var simpleClassField = definition.Fields.First(f => f.Name == nameof(SimpleClass.SimpleClassField));

            Assert.Equal(1337, handle.ReadField(intField).AsSpan().I32);
            Assert.Equal("Hello, world", Encoding.Unicode.GetString(handle
                .ReadField(stringField)
                .AsObjectHandle(_machine)
                .ReadStringData()
                .Bits));

            var embedded = handle.ReadField(simpleClassField).AsObjectHandle(_machine);

            Assert.Equal(1338, embedded.ReadField(intField).AsSpan().I32);
            Assert.Equal("Hello, mars", Encoding.Unicode.GetString(embedded
                .ReadField(stringField)
                .AsObjectHandle(_machine)
                .ReadStringData()
                .Bits));
            Assert.True(embedded.ReadField(simpleClassField).AsSpan().IsZero.ToBoolean());
        }

        [SuppressMessage("Usage", "xUnit1025:InlineData should be unique within the Theory it belongs to")]
        [Theory]
        [InlineData(new byte[] {0x01}, true)]
        [InlineData(new byte[] {0x00}, false)]
        [InlineData(new byte[] {0x12}, (byte) 0x12)]
        [InlineData(new byte[] {0x12}, (sbyte) 0x12)]
        [InlineData(new byte[] {0x34, 0x12}, (short) 0x1234)]
        [InlineData(new byte[] {0x34, 0x12}, (ushort) 0x1234)]
        [InlineData(new byte[] {0x78, 0x56, 0x34, 0x12}, (int) 0x12345678)]
        [InlineData(new byte[] {0x78, 0x56, 0x34, 0x12}, (uint) 0x12345678)]
        [InlineData(new byte[] {0xf0, 0xde, 0xbc, 0x9a, 0x78, 0x56, 0x34, 0x12}, (long) 0x123456789abcdef0)]
        [InlineData(new byte[] {0xf0, 0xde, 0xbc, 0x9a, 0x78, 0x56, 0x34, 0x12}, (ulong) 0x123456789abcdef0)]
        public void DeserializePrimitive(byte[] value, object expected)
        {
            object? result = _machine.ObjectMarshaller.ToObject(new BitVector(value), expected.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void DeserializeNullReferenceShouldReturnNull()
        {
            object? result = _machine.ObjectMarshaller.ToObject<object?>(_machine.ValueFactory.CreateNativeInteger(0));
            Assert.Null(result);
        }

        [Fact]
        public void DeserializeInjectedObjectShouldReturnSameObject()
        {
            object dummy = new();
            long address = _machine.ObjectMapMemory.GetOrCreateMapping(dummy).AddressRange.Start;
            
            object? result = _machine.ObjectMarshaller.ToObject<object?>(_machine.ValueFactory.CreateNativeInteger(address));
            Assert.Same(dummy, result);
        }
    }
}