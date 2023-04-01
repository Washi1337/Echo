using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Heap;
using Echo.Platforms.AsmResolver.Emulation.Runtime;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Heap
{
    public class StaticFieldStorageTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly ValueFactory _factory;
        private readonly StaticFieldStorage _storage;

        public StaticFieldStorageTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            
            _factory = new ValueFactory(fixture.MockModule, false);
            _storage = new StaticFieldStorage(_factory, 0x0100_0000);
        }

        [Fact]
        public void GetAddressShouldBeUniqueForUniqueFields()
        {
            var field1 = new FieldDefinition("Field1", FieldAttributes.Static,
                _fixture.MockModule.CorLibTypeFactory.Int32);
            var field2 = new FieldDefinition("Field2", FieldAttributes.Static,
                _fixture.MockModule.CorLibTypeFactory.Int32);

            long address1 = _storage.GetFieldAddress(field1);
            long address2 = _storage.GetFieldAddress(field1);
            long address3 = _storage.GetFieldAddress(field2);
            Assert.Equal(address1, address2);
            Assert.NotEqual(address1, address3);
        }

        [Fact]
        public void GetSpanOfPrimitiveShouldReflectSize()
        {
            var intField = new FieldDefinition("IntField", FieldAttributes.Static,
                _fixture.MockModule.CorLibTypeFactory.Int32);

            var intSpan = _storage.GetFieldSpan(intField);
            Assert.Equal(sizeof(int), intSpan.ByteCount);
        }

        [Fact]
        public void GetSpanOfObjectShouldReflectPointer()
        {
            var objectFIeld = new FieldDefinition("ObjectFIeld", FieldAttributes.Static,
                _fixture.MockModule.CorLibTypeFactory.Object);

            var intSpan = _storage.GetFieldSpan(objectFIeld);
            Assert.Equal((int) _factory.PointerSize, intSpan.ByteCount);
        }

        [Fact]
        public void GetSpanShouldBeUniqueForUniqueFields()
        {
            var field1 = new FieldDefinition("Field1", FieldAttributes.Static,
                _fixture.MockModule.CorLibTypeFactory.Int32);
            var field2 = new FieldDefinition("Field2", FieldAttributes.Static,
                _fixture.MockModule.CorLibTypeFactory.Int32);

            var span1 = _storage.GetFieldSpan(field1);
            var span2 = _storage.GetFieldSpan(field1);
            var span3 = _storage.GetFieldSpan(field2);

            span1.I32 = 1337;
            Assert.Equal(span1.I32, span2.I32);
            Assert.NotEqual(span1.I32, span3.I32);
        }
           

    }
}