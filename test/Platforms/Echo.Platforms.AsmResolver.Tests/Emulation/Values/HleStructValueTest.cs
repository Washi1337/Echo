using System;
using System.Linq;
using AsmResolver.DotNet;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Values
{
    public class HleStructValueTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly ICilRuntimeEnvironment _environment;

        public HleStructValueTest(MockModuleFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _environment = new MockCilRuntimeEnvironment(_fixture.MockModule, false);
        }
        
        [Fact]
        public void AllFieldsKnown()
        {
            var type = (TypeDefinition) _fixture.MockModule.LookupMember(typeof(SimpleStruct).MetadataToken);
            var value = new HleStructValue(_environment.ValueFactory, type.ToTypeSignature(), true);
            Assert.True(value.IsKnown);
        }
        
        [Fact]
        public void AllFieldsUnknown()
        {
            var type = (TypeDefinition) _fixture.MockModule.LookupMember(typeof(SimpleStruct).MetadataToken);
            var value = new HleStructValue(_environment.ValueFactory, type.ToTypeSignature(), false);
            Assert.False(value.IsKnown);
        }
        
        [Fact]
        public void OneFieldUnknown()
        {
            var type = (TypeDefinition) _fixture.MockModule.LookupMember(typeof(SimpleStruct).MetadataToken);
            var field = type.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
            var value = new HleStructValue(_environment.ValueFactory, type.ToTypeSignature(), true);
            value.SetFieldValue(field, new Integer32Value(0, 0));
            Assert.False(value.IsKnown);
        }
    }
}