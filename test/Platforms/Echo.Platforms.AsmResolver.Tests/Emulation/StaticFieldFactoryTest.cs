using System;
using System.Linq;
using AsmResolver.DotNet;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class StaticFieldFactoryTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _moduleFixture;

        public StaticFieldFactoryTest(MockModuleFixture moduleFixture)
        {
            _moduleFixture = moduleFixture ?? throw new ArgumentNullException(nameof(moduleFixture));
        }
        
        [Fact]
        public void GetValueOfStaticIntFieldOnFreshEnvironmentShouldContainIntegerValue()
        {
            var field = (FieldDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass)
                .GetField(nameof(SimpleClass.StaticIntField))
                .MetadataToken);
            
            var environment = new MockCilRuntimeEnvironment();
            var staticField = environment.StaticFieldFactory.Get(field);
            
            Assert.IsAssignableFrom<Integer32Value>(staticField.Value);
        }
        
        [Fact]
        public void GetValueOfStaticObjectFieldOnFreshEnvironmentShouldContainIntegerValue()
        {
            var field = (FieldDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass)
                .GetField(nameof(SimpleClass.StaticObjectField))
                .MetadataToken);
            
            var environment = new MockCilRuntimeEnvironment();
            var staticField = environment.StaticFieldFactory.Get(field);
            
            Assert.IsAssignableFrom<ObjectReference>(staticField.Value);
        }
    }
}