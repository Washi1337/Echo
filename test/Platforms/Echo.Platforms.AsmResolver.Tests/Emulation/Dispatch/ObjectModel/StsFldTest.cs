using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class StsFldTest : DispatcherTestBase
    {
        public StsFldTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }
        
        private void Verify(string fieldName, ICliValue newValue, IConcreteValue expectedValue)
        {
            var module = ModuleDefinition.FromFile(typeof(SimpleClass).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(SimpleClass).MetadataToken);
            var field = type.Fields.First(f => f.Name == fieldName);

            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var staticField = environment.StaticFieldFactory.Get(field);

            ExecutionContext.ProgramState.Stack.Push(newValue);
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Stsfld, field));

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, staticField.Value);
        }

        [Fact]
        public void WriteStaticIntField()
        {
            Verify(nameof(SimpleClass.StaticIntField), new I4Value(0x12345678), new Integer32Value(0x12345678));
        }

        [Fact]
        public void WriteStaticStringField()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldContents = environment.MemoryAllocator.GetStringValue("Hello, World!");
            var fieldValue = new OValue(fieldContents, true, environment.Is32Bit);
            Verify(nameof(SimpleClass.StaticStringField), fieldValue, new ObjectReference(fieldContents, environment.Is32Bit));
        }

        [Fact]
        public void WriteStaticInt16EnumField()
        {
            Verify(nameof(SimpleClass.StaticInt16Enum), new I4Value(1), new Integer16Value(1));
        }

        [Fact]
        public void WriteStaticInt32EnumField()
        {
            Verify(nameof(SimpleClass.StaticInt32Enum), new I4Value(1), new Integer32Value(1));
        }
    }
}