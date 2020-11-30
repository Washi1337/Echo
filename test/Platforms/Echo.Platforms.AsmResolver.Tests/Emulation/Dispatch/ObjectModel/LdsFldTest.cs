using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class LdsFldTest : DispatcherTestBase
    {
        public LdsFldTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        private void SetAndVerify(string fieldName, IConcreteValue fieldValue, ICliValue expectedValue)
        {
            var module = ModuleDefinition.FromFile(typeof(SimpleClass).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(SimpleClass).MetadataToken);
            var field = type.Fields.First(f => f.Name == fieldName);

            SetAndVerify(field, fieldValue, expectedValue);
        }

        private void SetAndVerify(IFieldDescriptor field, IConcreteValue fieldValue, ICliValue expectedValue)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            environment.StaticFieldFactory.Get(field).Value = fieldValue;
            Verify(field, expectedValue);
        }

        private void Verify(IFieldDescriptor field, ICliValue expectedValue)
        {
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldsfld, field));

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, ExecutionContext.ProgramState.Stack.Top);
        }

        [Fact]
        public void ReadStaticIntField()
        {
            var fieldValue = new Integer32Value(0x12345678);
            SetAndVerify(nameof(SimpleClass.StaticIntField), fieldValue, new I4Value(fieldValue.I32));
        }

        [Fact]
        public void ReadStaticStringField()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldValue = environment.ValueFactory.GetStringValue("Hello, World!");
            SetAndVerify(nameof(SimpleClass.StaticStringField), fieldValue, new OValue(fieldValue, true, environment.Is32Bit));
        }

        [Fact]
        public void ReadFromInt32EnumShouldResultInI4()
        {
            var fieldValue = new Integer32Value(1);
            SetAndVerify(nameof(SimpleClass.StaticInt32Enum), fieldValue, new I4Value(fieldValue.I32));
        }

        [Fact]
        public void ReadFromInt16EnumShouldResultInI4()
        {
            var fieldValue = new Integer16Value(1);
            SetAndVerify(nameof(SimpleClass.StaticInt16Enum), fieldValue, new I4Value(fieldValue.I16));
        }

        [Fact]
        public void ReadFromInt16EnumMemberShouldResultInI4()
        {
            var module = ModuleDefinition.FromFile(typeof(Int16Enum).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(Int16Enum).MetadataToken);
            var field = type.Fields.First(f => f.Name == nameof(Int16Enum.Member1));

            Verify(field, new I4Value((short) Int16Enum.Member1));
        }
    }
}