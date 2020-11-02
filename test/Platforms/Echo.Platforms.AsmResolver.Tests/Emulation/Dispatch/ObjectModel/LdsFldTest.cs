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

        private void Verify(string fieldName, IConcreteValue fieldValue, ICliValue expectedValue)
        {
            var module = ModuleDefinition.FromFile(typeof(SimpleClass).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(SimpleClass).MetadataToken);
            var field = type.Fields.First(f => f.Name == fieldName);

            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            environment.StaticFieldFactory.Get(field).Value = fieldValue;

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldsfld, field));

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, ExecutionContext.ProgramState.Stack.Top);
        }

        [Fact]
        public void ReadStaticIntField()
        {
            var fieldValue = new Integer32Value(0x12345678);
            Verify(nameof(SimpleClass.StaticIntField), fieldValue, new I4Value(fieldValue.I32));
        }

        [Fact]
        public void ReadStaticStringField()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var fieldValue = environment.MemoryAllocator.GetStringValue("Hello, World!");
            Verify(nameof(SimpleClass.StaticStringField), fieldValue, new OValue(fieldValue, true, environment.Is32Bit));
        }
    }
}