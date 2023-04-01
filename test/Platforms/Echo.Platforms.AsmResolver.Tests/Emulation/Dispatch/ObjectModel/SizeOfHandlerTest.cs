using System;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel;

public class SizeOfHandlerTest : CilOpCodeHandlerTestBase
{

    public SizeOfHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Theory]
    [InlineData(ElementType.I1, 1)]
    [InlineData(ElementType.I2, 2)]
    [InlineData(ElementType.I4, 4)]
    [InlineData(ElementType.I8, 8)]
    [InlineData(ElementType.U1, 1)]
    [InlineData(ElementType.U2, 2)]
    [InlineData(ElementType.U4, 4)]
    [InlineData(ElementType.U8, 8)]
    [InlineData(ElementType.R4, 4)]
    [InlineData(ElementType.R8, 8)]
    public void SizeOfPrimitiveValueType(ElementType elementType, int expected)
    {
        AssertCorrect(ModuleFixture.MockModule.CorLibTypeFactory.FromElementType(elementType)!.Type, expected);
    }

    [Theory]
    [InlineData(typeof(SimpleStruct))]
    [InlineData(typeof(SimpleClass))]
    public void SizeOfUserType(Type type)
    {
        var definition = ModuleFixture.MockModule.TopLevelTypes.First(t => t.IsTypeOf(type.Namespace, type.Name));
        uint size = Context.Machine.ValueFactory.GetTypeValueMemoryLayout(definition).Size;
        AssertCorrect(definition, (int) size);
    }
    
    [Fact]
    public void SizeOfNativeInteger()
    {
        AssertCorrect(
            ModuleFixture.MockModule.CorLibTypeFactory.IntPtr.Type, 
            (int) Context.Machine.ValueFactory.PointerSize);
    }


    [Fact]
    public void SizeOfReferenceType()
    {
        AssertCorrect(
            ModuleFixture.MockModule.CorLibTypeFactory.Object.Type, 
            (int) Context.Machine.ValueFactory.PointerSize);
    }

    private void AssertCorrect(ITypeDefOrRef type, int expected)
    {
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Sizeof, type));

        Assert.True(result.IsSuccess);
        var value = Assert.Single(Context.CurrentFrame.EvaluationStack);
        Assert.Equal(expected, value.Contents.AsSpan().I32);
    }

}