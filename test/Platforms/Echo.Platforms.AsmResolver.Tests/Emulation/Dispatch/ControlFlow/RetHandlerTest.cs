using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ControlFlow
{
    public class RetHandlerTest : CilOpCodeHandlerTestBase
    {
        public RetHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        [Fact]
        public void RetFromVoidShouldPopFromCallStack()
        {
            int currentFrameCount = Context.Thread.CallStack.Count;

            var instruction = new CilInstruction(CilOpCodes.Ret);
            var result = Dispatcher.Dispatch(Context, instruction);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(currentFrameCount - 1, Context.Thread.CallStack.Count);
        }
        
        [Fact]
        public void ReturnShouldPushValueOntoStackMarshalled()
        {
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(ModuleFixture.MockModule.CorLibTypeFactory.Int32));
            var frame = Context.Thread.CallStack.Push(method);
            frame.EvaluationStack.Push(new StackSlot(0x0123456789abcdef, StackSlotTypeHint.Integer));
            
            var calleeFrame = Context.CurrentFrame;
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ret));
            
            Assert.True(result.IsSuccess);
            Assert.NotSame(calleeFrame, Context.CurrentFrame);

            var value = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, value.TypeHint);
            Assert.Equal(32, value.Contents.Count);
            Assert.Equal(0x89abcdef, value.Contents.AsSpan().U32);
        }
    }
}