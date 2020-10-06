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
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class StFldTest : DispatcherTestBase
    {
        private readonly ModuleDefinition _module;
        
        public StFldTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
            _module = ModuleDefinition.FromFile(typeof(LdFldTest).Assembly.Location);
        }

        private TypeDefinition LookupTestType(Type type)
        {
            return (TypeDefinition) _module.LookupMember(type.MetadataToken);
        }

        private void Verify(string fieldName, ICliValue stackValue, IConcreteValue expectedValue)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;

            // Look up relevant metadata.
            var simpleClassType = LookupTestType(typeof(SimpleClass));
            var field = simpleClassType.Fields.First(f => f.Name == fieldName);

            // Create new virtual instance and push on stack. 
            var value = new HleObjectValue(simpleClassType.ToTypeSignature(), environment.Is32Bit);
            stack.Push(environment.CliMarshaller.ToCliValue(value, simpleClassType.ToTypeSignature()));
            stack.Push(stackValue);

            // Test stfld.
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Stfld, field));
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, value.GetFieldValue(field));
        }
        
        [Fact]
        public void WriteIntegerField()
        {
            const int rawValue = 1234;
            Verify(nameof(SimpleClass.IntField), new I4Value(rawValue), new Integer32Value(rawValue));
        }
        
        [Fact]
        public void WriteStringField()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldValue = environment.MemoryAllocator.GetStringValue("Hello, world!");
            var stackValue = environment.CliMarshaller.ToCliValue(fieldValue, _module.CorLibTypeFactory.String);
            Verify(nameof(SimpleClass.StringField), stackValue, fieldValue);
        }
        
        [Fact]
        public void WriteObjectReferenceFieldWithNullValue()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldValue = ObjectReference.Null(environment.Is32Bit);
            var stackValue = environment.CliMarshaller.ToCliValue(fieldValue, _module.CorLibTypeFactory.Object);
            Verify(nameof(SimpleClass.SimpleClassField), stackValue, fieldValue);
        }

        [Fact]
        public void WriteObjectReferenceFieldWithNonNullValue()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldContents = new HleObjectValue(LookupTestType(typeof(SimpleClass)).ToTypeSignature(), environment.Is32Bit);
            var fieldValue = new ObjectReference(fieldContents, environment.Is32Bit);
            var stackValue = environment.CliMarshaller.ToCliValue(fieldValue, _module.CorLibTypeFactory.Object);
            Verify(nameof(SimpleClass.SimpleClassField), stackValue, fieldValue);
        }

        [Fact]
        public void WriteToNullReferenceShouldThrow()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;

            // Look up relevant metadata.
            var simpleClassType = LookupTestType(typeof(SimpleClass));
            var intField = simpleClassType.Fields.First(f => f.Name == nameof(SimpleClass.IntField));

            // Push null.
            stack.Push(OValue.Null(environment.Is32Bit));
            stack.Push(new I4Value(1234));

            // Test.
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Stfld, intField));
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<NullReferenceException>(result.Exception);
        }
    }
}