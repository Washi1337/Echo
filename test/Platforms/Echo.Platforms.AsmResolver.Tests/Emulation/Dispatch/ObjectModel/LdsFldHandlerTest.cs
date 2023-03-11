using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel;

public class LdsFldHandlerTest : CilOpCodeHandlerTestBase
{
    public LdsFldHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void ReadStaticInt32()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        
        var field = new FieldDefinition("StaticInt32", FieldAttributes.Static,
            ModuleFixture.MockModule.CorLibTypeFactory.Int32);

        Context.Machine.StaticFieldStorage.GetFieldSpan(field).Write(1337);

        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldsfld, field));

        Assert.True(result.IsSuccess);
        Assert.Single(stack);
        Assert.Equal(1337, stack.Pop().Contents.AsSpan().I32);
    }

    [Fact]
    public void ReadStaticFloat64()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        
        var field = new FieldDefinition("StaticFloat64", FieldAttributes.Static,
            ModuleFixture.MockModule.CorLibTypeFactory.Double);

        Context.Machine.StaticFieldStorage.GetFieldSpan(field).Write(1337.0);

        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldsfld, field));

        Assert.True(result.IsSuccess);
        Assert.Single(stack);
        Assert.Equal(1337.0, stack.Pop().Contents.AsSpan().F64);
    }
}