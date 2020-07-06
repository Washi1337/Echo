using System;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class LdFldTest : DispatcherTestBase
    {
        private readonly ModuleDefinition _module;
        
        public LdFldTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
            _module = ModuleDefinition.FromFile(typeof(LdFldTest).Assembly.Location);
        }

        private TypeDefinition LookupTestType(Type type)
        {
            return (TypeDefinition) _module.LookupMember(type.MetadataToken);
        }

        private void Verify(string fieldName, IConcreteValue fieldValue, ICliValue expectedValue)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;

            // Look up relevant metadata.
            var simpleClassType = LookupTestType(typeof(SimpleClass));
            var field = simpleClassType.Fields.First(f => f.Name == fieldName);

            // Create new virtual instance and push on stack. 
            var value = new HighLevelObjectValue(simpleClassType.ToTypeSignature(), environment.Is32Bit);
            value.SetFieldValue(field, fieldValue);
            stack.Push(environment.CliMarshaller.ToCliValue(value, simpleClassType.ToTypeSignature()));

            // Test ldfld.
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldfld, field));
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, stack.Top);
        }


        [Fact]
        public void ReadIntegerField()
        {
            const int rawValue = 1234;
            Verify(nameof(SimpleClass.IntField), new Integer32Value(rawValue), new I4Value(rawValue));
        }

        [Fact]
        public void ReadStringField()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldValue = environment.MemoryAllocator.GetStringValue("Hello, world!");
            Verify(nameof(SimpleClass.StringField), fieldValue, new OValue(fieldValue, true, environment.Is32Bit));
        }

        [Fact]
        public void ReadObjectReferenceFieldWithNullValue()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldValue = ObjectReference.Null(environment.Is32Bit);
            Verify(nameof(SimpleClass.SimpleClassField), fieldValue, new OValue(fieldValue.ReferencedObject, true, environment.Is32Bit));
        }

        [Fact]
        public void ReadObjectReferenceFieldWithNonNullValue()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldContents = new HighLevelObjectValue(LookupTestType(typeof(SimpleClass)).ToTypeSignature(), environment.Is32Bit);
            var fieldValue = new ObjectReference(fieldContents, environment.Is32Bit);
            Verify(nameof(SimpleClass.SimpleClassField), fieldValue, new OValue(fieldValue.ReferencedObject, true, environment.Is32Bit));
        }

        [Fact]
        public void ReadFromNullReferenceShouldThrow()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;

            // Look up relevant metadata.
            var simpleClassType = LookupTestType(typeof(SimpleClass));
            var intField = simpleClassType.Fields.First(f => f.Name == nameof(SimpleClass.IntField));

            // Push null.
            stack.Push(OValue.Null(environment.Is32Bit));

            // Test.
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldfld, intField));
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<NullReferenceException>(result.Exception);
        }
        
    }
}