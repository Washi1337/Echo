using System.Runtime.InteropServices;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Constants
{
    public class LdStrHandlerTest : CilOpCodeHandlerTestBase
    {
        public LdStrHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void InstructionShouldPushPointerToString()
        {
            Assert.Empty(Context.CurrentFrame.EvaluationStack);

            const string operand = "Hello, world!";
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldstr, operand));
            
            Assert.True(result.IsSuccess);
            
            var value = Assert.Single(Context.CurrentFrame.EvaluationStack);
            Assert.Equal(StackSlotTypeHint.Integer, value.TypeHint);

            var stringObject = value.Contents.ToObjectHandle(Context.Machine);
            var stringData = stringObject.ReadStringData();
            Assert.Equal(operand, new string(MemoryMarshal.Cast<byte, char>(stringData.Bits)));
        }

        [Fact]
        public void InstructionShouldPushInternedStringPointers()
        {
            Assert.Empty(Context.CurrentFrame.EvaluationStack);

            const string operand = "Hello, world!";
            Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldstr, operand));
            Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ldstr, operand));


            var stack = Context.CurrentFrame.EvaluationStack;
            Assert.Equal(stack[1].Contents.AsSpan().U32, stack[0].Contents.AsSpan().U32);
        }
    }
}