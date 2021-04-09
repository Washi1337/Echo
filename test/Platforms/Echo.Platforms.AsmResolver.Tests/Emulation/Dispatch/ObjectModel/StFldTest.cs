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
            _module = moduleFixture.MockModule;
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
            var objectRef = environment.ValueFactory.CreateObject(simpleClassType.ToTypeSignature(), true);
            var contents = (IDotNetStructValue) objectRef.ReferencedObject;
            stack.Push(environment.CliMarshaller.ToCliValue(objectRef, simpleClassType.ToTypeSignature()));
            stack.Push(stackValue);

            // Test stfld.
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Stfld, field));
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, contents.GetFieldValue(field));
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
            var fieldContents = environment.ValueFactory.GetStringValue("Hello, world!");
            var fieldValue = new ObjectReference(fieldContents, environment.Is32Bit);
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
            var fieldValue = environment.ValueFactory.CreateObject(
                LookupTestType(typeof(SimpleClass)).ToTypeSignature(), 
                true);
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

        private void VerifyUndocumentedWriteStatic(ICliValue instanceObject, ICliValue stackValue, IConcreteValue expectedValue)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;

            // Set initial field value.
            var simpleClassType = LookupTestType(typeof(SimpleClass));
            var field = simpleClassType.Fields.First(f => f.Name == nameof(SimpleClass.StaticIntField));

            // Push random object.
            stack.Push(instanceObject);
            stack.Push(stackValue);

            // Test.
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Stfld, field));
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, environment.StaticFieldFactory.Get(field).Value);
        }

        [Fact]
        public void UndocumentedWriteStaticFromAnyObjectReferenceShouldNotThrow()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var instanceObject = new OValue(
                environment.ValueFactory.GetStringValue("Hello, world"),
                true,
                environment.Is32Bit);
            
            VerifyUndocumentedWriteStatic(instanceObject, 
                new I4Value(0x12345678), 
                new Integer32Value(0x12345678));
        }

        [Fact]
        public void UndocumentedWriteStaticFromNullReferenceShouldNotThrow()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            VerifyUndocumentedWriteStatic(
                OValue.Null(environment.Is32Bit), 
                new I4Value(0x12345678),
                new Integer32Value(0x12345678));
        }
    }
}