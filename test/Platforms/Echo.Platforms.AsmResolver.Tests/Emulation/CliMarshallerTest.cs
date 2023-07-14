using System.Linq;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation;

public class CliMarshallerTest : IClassFixture<MockModuleFixture>
{
    private readonly MockModuleFixture _fixture;
    private readonly ValueFactory _factory;
    private readonly CliMarshaller _marshaller;

    public CliMarshallerTest(MockModuleFixture fixture)
    {
        _fixture = fixture;
        _factory = new ValueFactory(_fixture.MockModule, false);
        _marshaller = new CliMarshaller(_factory);
    }

    [Fact]
    public void MarshalEnumToStackShouldMarshalAsInteger()
    {
        var int32EnumType = _fixture.MockModule.TopLevelTypes.First(x => x.Name == nameof(Int32Enum));
        var marshalledEnum = _marshaller.ToCliValue(new BitVector(0x1337), int32EnumType.ToTypeSignature());
        
        Assert.Equal(StackSlotTypeHint.Integer, marshalledEnum.TypeHint);
    }
}