using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Platforms.AsmResolver.Emulation.Runtime;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Runtime;

public class RuntimeTypeManagerTest : IClassFixture<MockModuleFixture>
{
    private readonly MockModuleFixture _fixture;
    private readonly RuntimeTypeManager _manager;

    public RuntimeTypeManagerTest(MockModuleFixture fixture)
    {
        _fixture = fixture;
        _manager = new RuntimeTypeManager(fixture.MockModule, false);
    }

    [Fact]
    public void ObjectMethodTable()
    {
        var table = _manager.GetMethodTable(_fixture.CurrentTestModule.CorLibTypeFactory.Object);
        Assert.Null(table.BaseMethodTable);
        Assert.Equal(new[]
        {
            "Finalize",
            "ToString",
            "Equals",
            "GetHashCode"
        }, table.VTable.Select(x => x.Name?.Value));
    }
    
    [Fact]
    public void NoOverridesMethodTable()
    {
        var type = _fixture.CurrentTestModule.LookupMember<TypeDefinition>(typeof(NoOverrides).MetadataToken);
        var table = _manager.GetMethodTable(type);
        Assert.NotNull(table.BaseMethodTable);
        Assert.Equal(new[]
        {
            "Finalize",
            "ToString",
            "Equals",
            "GetHashCode"
        }, table.VTable.Select(x => x.Name?.Value));
    }

    [Fact]
    public void ImmediateOverridesMethodTableShouldReuseSlot()
    {
        var type = _fixture.CurrentTestModule.LookupMember<TypeDefinition>(typeof(ImmediateOverride).MetadataToken);
        var table = _manager.GetMethodTable(type);
        Assert.NotNull(table.BaseMethodTable);
        Assert.All(table.VTable, method =>
        {
            var actualDeclaringType = method.DeclaringType!;
            var expectedDeclaringType = method.Name == "ToString" 
                ? type
                : table.BaseMethodTable!.Type.ToTypeDefOrRef();

            Assert.Equal(expectedDeclaringType, actualDeclaringType, SignatureComparer.Default);
        });
    }

    [Fact]
    public void IndirectOverridesMethodTableShouldReuseSlot()
    {
        var type = _fixture.CurrentTestModule.LookupMember<TypeDefinition>(typeof(ImmediateOverride).MetadataToken);
        var table = _manager.GetMethodTable(type);
        Assert.NotNull(table.BaseMethodTable);
        Assert.All(table.VTable, method =>
        {
            var actualDeclaringType = method.DeclaringType!;
            var expectedDeclaringType = method.Name == "ToString" 
                ? type
                : table.BaseMethodTable!.Type.ToTypeDefOrRef();

            Assert.Equal(expectedDeclaringType, actualDeclaringType, SignatureComparer.Default);
        });
    }

    [Fact]
    public void SignatureMismatchShouldAllocateNewSlot()
    {
        var type = _fixture.CurrentTestModule.LookupMember<TypeDefinition>(typeof(SignatureMismatch).MetadataToken);
        var table = _manager.GetMethodTable(type);
        Assert.NotNull(table.BaseMethodTable);
        Assert.Equal(new[]
        {
            "Finalize",
            "ToString",
            "Equals",
            "GetHashCode",
            "ToString",
        }, table.VTable.Select(x => x.Name?.Value));
    }

    [Fact]
    public void ShadowMethodShouldOccupyNewSlot()
    {
        var type = _fixture.CurrentTestModule.LookupMember<TypeDefinition>(typeof(ShadowMethod).MetadataToken);
        var table = _manager.GetMethodTable(type);
        Assert.NotNull(table.BaseMethodTable);
        Assert.Equal(new[]
        {
            "Finalize",
            "ToString",
            "Equals",
            "GetHashCode",
            "ToString",
        }, table.VTable.Select(x => x.Name?.Value));
        
        Assert.Equal(table.BaseMethodTable!.Type.ToTypeDefOrRef(), table.VTable[1].DeclaringType!.ToTypeDefOrRef(), SignatureComparer.Default);
        Assert.Equal(type, table.VTable[4].DeclaringType!.ToTypeDefOrRef(), SignatureComparer.Default);
    }

    class NoOverrides;

    class ImmediateOverride
    {
        public override string ToString() => "Test";
    }

    class SignatureMismatch
    {
        public virtual int ToString() => 1337;
    }

    class ShadowMethod
    {
        public new virtual string ToString() => "Test";
    }

    class Intermediate;

    class IndirectOverride : Intermediate
    {
        public override string ToString() => "Test";
    }

}