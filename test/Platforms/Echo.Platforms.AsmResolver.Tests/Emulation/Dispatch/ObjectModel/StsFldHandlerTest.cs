using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class StsFldHandlerTest : CilOpCodeHandlerTestBase
    {
        public StsFldHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void WriteStaticInt32()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
        
            var field = new FieldDefinition("StaticInt32", FieldAttributes.Static,
                ModuleFixture.MockModule.CorLibTypeFactory.Int32);

            stack.Push(new StackSlot(1337, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stsfld, field));

            Assert.True(result.IsSuccess);
            Assert.Empty(stack);
            Assert.Equal(1337, Context.Machine.StaticFields.GetFieldSpan(field).I32);
        }

        [Fact]
        public void WriteStaticFloat64()
        {
            var stack = Context.CurrentFrame.EvaluationStack;
        
            var field = new FieldDefinition("StaticFloat64", FieldAttributes.Static,
                ModuleFixture.MockModule.CorLibTypeFactory.Double);

            stack.Push(new StackSlot(1337.0, StackSlotTypeHint.Float));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Stsfld, field));

            Assert.True(result.IsSuccess);
            Assert.Empty(stack);
            Assert.Equal(1337.0, Context.Machine.StaticFields.GetFieldSpan(field).F64);
        }
    }
}