using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Misc
{
    public class DupHandlerTest : CilOpCodeHandlerTestBase
    {
        public DupHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void DuplicateInteger()
        {
            int currentPc = Context.CurrentFrame.ProgramCounter;

            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(new BitVector(1337), StackSlotTypeHint.Integer));
            
            Assert.Single(stack);

            var instruction = new CilInstruction(CilOpCodes.Dup);
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(currentPc + instruction.Size, Context.CurrentFrame.ProgramCounter);

            Assert.Equal(2, stack.Count);
            Assert.Equal(stack[0].TypeHint, stack[1].TypeHint);
            Assert.Equal(stack[0].Contents.AsSpan().I32, stack[1].Contents.AsSpan().I32);
        }
        
        [Fact]
        public void DuplicateFloat()
        {
            int currentPc = Context.CurrentFrame.ProgramCounter;

            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(new BitVector(1337.0), StackSlotTypeHint.Float));
            
            Assert.Single(stack);

            var instruction = new CilInstruction(CilOpCodes.Dup);
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(currentPc + instruction.Size, Context.CurrentFrame.ProgramCounter);

            Assert.Equal(2, stack.Count);
            Assert.Equal(stack[0].TypeHint, stack[1].TypeHint);
            Assert.Equal(stack[0].Contents.AsSpan().F64, stack[1].Contents.AsSpan().F64);
        }
    }
}