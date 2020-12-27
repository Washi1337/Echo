using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arrays
{
    public class NewArrTest : DispatcherTestBase
    {
        public NewArrTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Theory]
        [InlineData(ElementType.I)]
        [InlineData(ElementType.U)]
        [InlineData(ElementType.I1)]
        [InlineData(ElementType.I2)]
        [InlineData(ElementType.I4)]
        [InlineData(ElementType.I8)]
        [InlineData(ElementType.U1)]
        [InlineData(ElementType.U2)]
        [InlineData(ElementType.U4)]
        [InlineData(ElementType.U8)]
        [InlineData(ElementType.R4)]
        [InlineData(ElementType.R8)]
        [InlineData(ElementType.String)]
        [InlineData(ElementType.Object)]
        public void NewCorLibValueTypeArray(ElementType elementType)
        {
            const int length = 10;
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var elementTypeSig = environment.Module.CorLibTypeFactory.FromElementType(elementType);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(length));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Newarr, elementTypeSig.Type));
            
            Assert.True(result.IsSuccess);
            var reference = Assert.IsAssignableFrom<OValue>(stack.Top);
            var dotNetArray = Assert.IsAssignableFrom<IDotNetArrayValue>(reference.ReferencedObject);
            
            Assert.Equal(elementTypeSig.FullName, dotNetArray.Type.GetUnderlyingTypeDefOrRef().FullName);
            Assert.Equal(length, dotNetArray.Length);
        }
        
    }
}