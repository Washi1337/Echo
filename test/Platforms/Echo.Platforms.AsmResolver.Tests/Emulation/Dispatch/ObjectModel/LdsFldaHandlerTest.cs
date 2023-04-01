using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel;

public class LdsFldaHandlerTest : CilOpCodeHandlerTestBase
{
    public LdsFldaHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void GetStaticInt32()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        
        var field = new FieldDefinition("StaticInt32", FieldAttributes.Static,
            ModuleFixture.MockModule.CorLibTypeFactory.Int32);

        long address = Context.Machine.StaticFields.GetFieldAddress(field);

        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldsflda, field));

        Assert.True(result.IsSuccess);
        Assert.Single(stack);
        Assert.Equal(address, stack.Pop().Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit));
    }
}