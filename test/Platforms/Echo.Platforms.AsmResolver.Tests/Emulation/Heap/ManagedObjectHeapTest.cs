using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Concrete;
using Echo.Concrete.Memory.Heap;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Heap;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Heap
{
    public class ManagedObjectHeapTest : IClassFixture<MockModuleFixture>
    {
        private static readonly SignatureComparer Comparer = new();
        private readonly MockModuleFixture _fixture;
        private readonly ValueFactory _factory;
        private readonly ManagedObjectHeap _objectHeap;

        public ManagedObjectHeapTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            _factory = new ValueFactory(fixture.CurrentTestModule, false);
            _objectHeap = new ManagedObjectHeap(new BasicHeap(1000), _factory);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello, world!")]
        public void StringObjects(string value)
        {
            long address = _objectHeap.AllocateString(value);
            var objectSpan = _objectHeap.GetObjectSpan(address);

            Assert.Equal(_factory.ContextModule.CorLibTypeFactory.String,
                _factory.ClrMockMemory.MethodTables.GetObject(objectSpan.SliceObjectMethodTable(_factory).I64));
            Assert.Equal(value.Length, objectSpan.SliceStringLength(_factory).I32);
            Assert.Equal(value, new string(MemoryMarshal.Cast<byte, char>(objectSpan.SliceStringData(_factory).Bits)));
        }

        [Theory]
        [InlineData("Hello, world!")]
        public unsafe void ValidateObjectSlicesAreCorrect(string value)
        {
            // Beautiful pointer abuse to read the raw data of the string.
            var rawObjectAddress = *(nint*) Unsafe.AsPointer(ref value);
            byte[] data = new byte[sizeof(nint) + sizeof(int) + value.Length * sizeof(char)];
            Marshal.Copy(rawObjectAddress, data, 0, data.Length);

            var objectSpan = new BitVector(data).AsSpan();
            Assert.Equal(value.Length, objectSpan.SliceStringLength(_factory).I32);
            Assert.Equal(value, new string(MemoryMarshal.Cast<byte, char>(objectSpan.SliceStringData(_factory).Bits)));
        }

        [Fact]
        public void IntArray()
        {
            var elementType = _fixture.MockModule.CorLibTypeFactory.Int32;

            // Allocate empty int array of 10 elements.
            long address = _objectHeap.AllocateSzArray(elementType, 10, true);
            var objectSpan = _objectHeap.GetObjectSpan(address);

            // Set elements 0 to 10.
            for (int i = 0; i < 10; i++)
            {
                var elementSpan = objectSpan.SliceArrayElement(_factory, elementType, i);
                elementSpan.I32 = i;
            }

            // Verify type
            Assert.Equal(
                _factory.ContextModule.CorLibTypeFactory.Int32.MakeSzArrayType(),
                _factory.ClrMockMemory.MethodTables.GetObject(objectSpan.SliceObjectMethodTable(_factory).I64),
                Comparer);
            
            // Verify length.
            var lengthSpan = objectSpan.SliceArrayLength(_factory);
            Assert.Equal(10, lengthSpan.I32);

            // Verify elements.
            int[] elements = MemoryMarshal.Cast<byte, int>(objectSpan.SliceArrayData(_factory).Bits).ToArray();
            Assert.Equal(new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, elements);
        }

        [Fact]
        public void BoxedStruct()
        {
            var elementType = _fixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleStruct));

            // Allocate boxed struct.
            long address = _objectHeap.AllocateObject(elementType, true);
            var objectSpan = _objectHeap.GetObjectSpan(address);

            // Set fields
            var dataSpan = objectSpan.SliceObjectData(_factory);
            for (int i = 0; i < elementType.Fields.Count; i++)
            {
                var fieldSpan = dataSpan.SliceStructField(_factory, elementType.Fields[i]);
                fieldSpan.I32 = i;
            }

            // Verify type
            Assert.Equal(
                elementType,
                _factory.ClrMockMemory.MethodTables.GetObject(objectSpan.SliceObjectMethodTable(_factory).I64),
                Comparer);

            // Verify contents.
            var actualValue = MemoryMarshal.Cast<byte, SimpleStruct>(dataSpan.Bits)[0];
            Assert.Equal(new SimpleStruct(0, 1, 2), actualValue);
        }

        [Fact]
        public void ClassObject()
        {
            var elementType = _fixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(RecursiveClass));
            var tagField = elementType.Fields.First(f => f.Name == nameof(RecursiveClass.Tag));
            var itemField = elementType.Fields.First(f => f.Name == nameof(RecursiveClass.Item));

            // Allocate embedded object.
            long address2 = _objectHeap.AllocateObject(elementType, true);
            var objectSpan = _objectHeap.GetObjectSpan(address2);
            var dataSpan = objectSpan.SliceObjectData(_factory);
            
            var fieldSpan = dataSpan.SliceStructField(_factory, tagField);
            fieldSpan.I32 = 0x1337;
            
            // Allocate wrapped object.
            long address1 = _objectHeap.AllocateObject(elementType, true);
            objectSpan = _objectHeap.GetObjectSpan(address1);
            
            // Set fields
            dataSpan = objectSpan.SliceObjectData(_factory);
            fieldSpan = dataSpan.SliceStructField(_factory, tagField);
            fieldSpan.I32 = 0x1337;
            fieldSpan = dataSpan.SliceStructField(_factory, itemField);
            fieldSpan.I64 = address2;

            // Verify type
            Assert.Equal(
                elementType,
                _factory.ClrMockMemory.MethodTables.GetObject(objectSpan.SliceObjectMethodTable(_factory).I64),
                Comparer);
        }
    }
}