using System;
using System.Linq;
using System.Text;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
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

        Context.Machine.StaticFields.GetFieldSpan(field).Write(1337);

        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldsfld, field));

        Assert.True(result.IsSuccess);
        Assert.Equal(1337, Assert.Single(stack).Contents.AsSpan().I32);
    }

    [Fact]
    public void ReadStaticFloat64()
    {
        var stack = Context.CurrentFrame.EvaluationStack;
        
        var field = new FieldDefinition("StaticFloat64", FieldAttributes.Static,
            ModuleFixture.MockModule.CorLibTypeFactory.Double);

        Context.Machine.StaticFields.GetFieldSpan(field).Write(1337.0);

        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldsfld, field));

        Assert.True(result.IsSuccess);
        Assert.Equal(1337.0, Assert.Single(stack).Contents.AsSpan().F64);
    }

    [Fact]
    public void ReadStaticInt32WithInitializer()
    {
        // Lookup metadata.
        var type = ModuleFixture.MockModule.LookupMember<TypeDefinition>(typeof(ClassWithInitializer).MetadataToken);
        var cctor = type.GetStaticConstructor();
        var field = type.Fields.First(x => x.Name == nameof(ClassWithInitializer.Field));

        // Load static field.
        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldsfld, field));
        
        // Verify we jumped into the cctor.
        Assert.True(result.IsSuccess);
        Assert.Same(cctor, Context.Thread.CallStack.Peek(0).Method);

        // Finish execution of the cctor.
        Context.Thread.StepOut();
        
        // Load static field again.
        result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldsfld, field));
        
        // Verify we did not jump into the cctor.
        Assert.True(result.IsSuccess);
        Assert.NotSame(cctor, Context.Thread.CallStack.Peek(0).Method);
        
        // Verify that a string is pushed.
        var slot = Assert.Single(Context.CurrentFrame.EvaluationStack).Contents.AsObjectHandle(Context.Machine);
        Assert.False(slot.IsNull);
        Assert.Equal(ClassWithInitializer.Field, Encoding.Unicode.GetString(slot.ReadStringData().Bits));
    }

    [Fact]
    public void ReadStaticInt32WithFieldRva()
    {
        var field = new FieldDefinition(
            "StaticInt32WithFieldRva", 
            FieldAttributes.Static | FieldAttributes.HasFieldRva,
            ModuleFixture.MockModule.CorLibTypeFactory.Int32
        );
        field.FieldRva = new DataSegment(BitConverter.GetBytes(1337));

        var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldsfld, field));

        Assert.True(result.IsSuccess);
        Assert.Equal(1337, Assert.Single(Context.CurrentFrame.EvaluationStack).Contents.AsSpan().I32);
    }
}