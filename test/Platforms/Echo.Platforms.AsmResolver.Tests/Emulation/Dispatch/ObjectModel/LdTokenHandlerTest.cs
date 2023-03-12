using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel;

public class LdTokenHandlerTest : CilOpCodeHandlerTestBase
{
    public LdTokenHandlerTest(MockModuleFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void PushTypeHandle()
    {
        var type = ModuleFixture.MockModule.LookupMember<TypeDefinition>(new MetadataToken(TableIndex.TypeDef, 1));
        
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldtoken, type));

        Assert.True(result.IsSuccess);
        long handle = Assert.Single(Context.CurrentFrame.EvaluationStack).Contents
            .AsSpan()
            .ReadNativeInteger(Context.Machine.Is32Bit);
        
        Assert.Equal(type, Context.Machine.ValueFactory.ClrMockMemory.MethodTables.GetObject(handle));
    }

    [Fact]
    public void PushMethodHandle()
    {
        var method = ModuleFixture.MockModule.LookupMember(new MetadataToken(TableIndex.Method, 1));
        
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldtoken, method));

        Assert.True(result.IsSuccess);
        long handle = Assert.Single(Context.CurrentFrame.EvaluationStack).Contents
            .AsSpan()
            .ReadNativeInteger(Context.Machine.Is32Bit);
        
        Assert.Equal(method, Context.Machine.ValueFactory.ClrMockMemory.Methods.GetObject(handle));
    }

    [Fact]
    public void PushFieldHandle()
    {
        var field = ModuleFixture.MockModule.LookupMember(new MetadataToken(TableIndex.Field, 1));
        
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldtoken, field));

        Assert.True(result.IsSuccess);
        long handle = Assert.Single(Context.CurrentFrame.EvaluationStack).Contents
            .AsSpan()
            .ReadNativeInteger(Context.Machine.Is32Bit);
        
        Assert.Equal(field, Context.Machine.ValueFactory.ClrMockMemory.Fields.GetObject(handle));
    }
}