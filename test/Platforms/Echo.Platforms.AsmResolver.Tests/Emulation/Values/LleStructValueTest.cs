using System;
using System.Linq;
using AsmResolver.DotNet;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Values
{
    public class LleStructValueTest: IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly ICilRuntimeEnvironment _environment;

        public LleStructValueTest(MockModuleFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _environment = new MockCilRuntimeEnvironment(_fixture.MockModule, false);
        }
        
        [Fact]
        public unsafe void AllFieldsKnown()
        {
            var type = (TypeDefinition) _fixture.MockModule.LookupMember(typeof(SimpleStruct).MetadataToken);
            var contents = new MemoryBlockValue(
                new Memory<byte>(new byte[sizeof(SimpleStruct)]),
                _environment.Is32Bit);

            var value = new LleStructValue(_environment.ValueFactory, type.ToTypeSignature(), contents);
            Assert.True(value.IsKnown);
        }
        
        [Fact]
        public unsafe void AllFieldsUnknown()
        {
            var type = (TypeDefinition) _fixture.MockModule.LookupMember(typeof(SimpleStruct).MetadataToken);
            var contents = new MemoryBlockValue(
                new Memory<byte>(new byte[sizeof(SimpleStruct)]),
                new Memory<byte>(new byte[sizeof(SimpleStruct)]),
                _environment.Is32Bit);

            var value = new LleStructValue(_environment.ValueFactory, type.ToTypeSignature(), contents);
            Assert.False(value.IsKnown);
        }
        
        [Fact]
        public unsafe void OneFieldUnknown()
        {
            var type = (TypeDefinition) _fixture.MockModule.LookupMember(typeof(SimpleStruct).MetadataToken);
            var field = type.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
            var contents = new MemoryBlockValue(
                new Memory<byte>(new byte[sizeof(SimpleStruct)]),
                _environment.Is32Bit);

            var value = new LleStructValue(_environment.ValueFactory, type.ToTypeSignature(), contents);
            value.SetFieldValue(field, new Integer32Value(0, 0));
            Assert.False(value.IsKnown);
        }
    }
}