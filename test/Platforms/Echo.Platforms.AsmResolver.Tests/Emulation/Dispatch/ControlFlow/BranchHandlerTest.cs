using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ControlFlow
{
    public class BranchHandlerTest : CilOpCodeHandlerTestBase
    {
        public BranchHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void BrShouldJumpToOperand()
        {
            var instruction = new CilInstruction(CilOpCodes.Br, new CilOffsetLabel(0x1337));
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0x1337, Context.CurrentFrame.ProgramCounter);
        }

        [Theory]
        [InlineData(CilCode.Brtrue, "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Brtrue, "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Brtrue, "00001000000000100010000000000000", true)]
        [InlineData(CilCode.Brtrue, "00001???????????????????????????", true)]
        [InlineData(CilCode.Brtrue_S, "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Brtrue_S, "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Brtrue_S, "00001000000000100010000000000000", true)]
        [InlineData(CilCode.Brtrue_S, "00001???????????????????????????", true)]
        [InlineData(CilCode.Brfalse, "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Brfalse, "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Brfalse, "00001000000000100010000000000000", false)]
        [InlineData(CilCode.Brfalse, "00001???????????????????????????", false)]
        [InlineData(CilCode.Brfalse_S, "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Brfalse_S, "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Brfalse_S, "00001000000000100010000000000000", false)]
        [InlineData(CilCode.Brfalse_S, "00001???????????????????????????", false)]
        public void UnaryBranchTest(CilCode code, string stackValue, bool expectedToJump)
        {
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(
                BitVector.ParseBinary(stackValue),
                StackSlotTypeHint.Integer));
            
            var instruction = new CilInstruction(code.ToOpCode(), new CilOffsetLabel(0x1337));
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedToJump ? 0x1337 : instruction.Size, Context.CurrentFrame.ProgramCounter);
        }

        [Theory]
        [InlineData(CilCode.Beq, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Beq, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Beq, "00000000000000000000000000000001", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Beq, "00000000000000000???????????????", "10000000000000000???????????????", false)]
        [InlineData(CilCode.Beq_S, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Beq_S, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Beq_S, "00000000000000000000000000000001", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Beq_S, "00000000000000000???????????????", "10000000000000000???????????????", false)]
        
        [InlineData(CilCode.Bne_Un, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Bne_Un, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Bne_Un, "00000000000000000000000000000001", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Bne_Un, "00000000000000000???????????????", "10000000000000000???????????????", true)]
        [InlineData(CilCode.Bne_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Bne_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Bne_Un_S, "00000000000000000000000000000001", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Bne_Un_S, "00000000000000000???????????????", "10000000000000000???????????????", true)]
        
        [InlineData(CilCode.Blt, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Blt, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Blt, "0000000000000000000?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Blt, "0000000000000000010?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Blt, "11111111111111111111111111111111", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Blt_S, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Blt_S, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Blt_S, "0000000000000000000?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Blt_S, "0000000000000000010?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Blt_S, "11111111111111111111111111111111", "00000000000000000000000000000000", true)]
        
        [InlineData(CilCode.Blt_Un, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Blt_Un, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Blt_Un, "0000000000000000000?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Blt_Un, "0000000000000000010?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Blt_Un, "11111111111111111111111111111111", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Blt_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Blt_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Blt_Un_S, "0000000000000000000?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Blt_Un_S, "0000000000000000010?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Blt_Un_S, "11111111111111111111111111111111", "00000000000000000000000000000000", false)]
        
        [InlineData(CilCode.Ble, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Ble, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Ble, "0000000000000000000?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Ble, "0000000000000000010?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Ble, "11111111111111111111111111111111", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Ble_S, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Ble_S, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Ble_S, "0000000000000000000?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Ble_S, "0000000000000000010?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Ble_S, "11111111111111111111111111111111", "00000000000000000000000000000000", true)]
        
        [InlineData(CilCode.Ble_Un, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Ble_Un, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Ble_Un, "0000000000000000000?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Ble_Un, "0000000000000000010?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Ble_Un, "11111111111111111111111111111111", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Ble_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Ble_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000001", true)]
        [InlineData(CilCode.Ble_Un_S, "0000000000000000000?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Ble_Un_S, "0000000000000000010?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Ble_Un_S, "11111111111111111111111111111111", "00000000000000000000000000000000", false)]
        
        [InlineData(CilCode.Bgt, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Bgt, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Bgt, "0000000000000000000?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Bgt, "0000000000000000010?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Bgt, "11111111111111111111111111111111", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Bgt_S, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Bgt_S, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Bgt_S, "0000000000000000000?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Bgt_S, "0000000000000000010?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Bgt_S, "11111111111111111111111111111111", "00000000000000000000000000000000", false)]
        
        [InlineData(CilCode.Bgt_Un, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Bgt_Un, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Bgt_Un, "0000000000000000000?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Bgt_Un, "0000000000000000010?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Bgt_Un, "11111111111111111111111111111111", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Bgt_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Bgt_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Bgt_Un_S, "0000000000000000000?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Bgt_Un_S, "0000000000000000010?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Bgt_Un_S, "11111111111111111111111111111111", "00000000000000000000000000000000", true)]
        
        [InlineData(CilCode.Bge, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Bge, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Bge, "0000000000000000000?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Bge, "0000000000000000010?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Bge, "11111111111111111111111111111111", "00000000000000000000000000000000", false)]
        [InlineData(CilCode.Bge_S, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Bge_S, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Bge_S, "0000000000000000000?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Bge_S, "0000000000000000010?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Bge_S, "11111111111111111111111111111111", "00000000000000000000000000000000", false)]
        
        [InlineData(CilCode.Bge_Un, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Bge_Un, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Bge_Un, "0000000000000000000?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Bge_Un, "0000000000000000010?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Bge_Un, "11111111111111111111111111111111", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Bge_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000000", true)]
        [InlineData(CilCode.Bge_Un_S, "00000000000000000000000000000000", "00000000000000000000000000000001", false)]
        [InlineData(CilCode.Bge_Un_S, "0000000000000000000?????????????", "00000000000000000010000000000000", false)]
        [InlineData(CilCode.Bge_Un_S, "0000000000000000010?????????????", "00000000000000000010000000000000", true)]
        [InlineData(CilCode.Bge_Un_S, "11111111111111111111111111111111", "00000000000000000000000000000000", true)]
        public void BinaryBranchTest(CilCode code, string left, string right, bool expectedToJump)
        {
            var stack = Context.CurrentFrame.EvaluationStack;
            stack.Push(new StackSlot(BitVector.ParseBinary(left), StackSlotTypeHint.Integer));
            stack.Push(new StackSlot(BitVector.ParseBinary(right), StackSlotTypeHint.Integer));
            
            var instruction = new CilInstruction(code.ToOpCode(), new CilOffsetLabel(0x1337));
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedToJump ? 0x1337 : instruction.Size, Context.CurrentFrame.ProgramCounter);
        }
        
    }
}