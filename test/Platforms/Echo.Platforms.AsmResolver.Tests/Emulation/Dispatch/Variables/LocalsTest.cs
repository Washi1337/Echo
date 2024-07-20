using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Variables
{
    public class LocalsTest : CilOpCodeHandlerTestBase
    {
        public LocalsTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        private void PrepareMethodWithLocal(int localCount, TypeSignature localType)
        {
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("DummyMethod", MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void));

            var body = new CilMethodBody(method);
            for (int i = 0; i <= localCount; i++)
                body.LocalVariables.Add(new CilLocalVariable(localType));
            method.CilMethodBody = body;

            Context.Thread.CallStack.Push(method);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void PushInt32Local(int localIndex)
        {
            PrepareMethodWithLocal(localIndex, ModuleFixture.MockModule.CorLibTypeFactory.Int32);
            Context.CurrentFrame.WriteLocal(localIndex, new BitVector(1337).AsSpan());
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Ldloc, Context.CurrentFrame.Body!.LocalVariables[localIndex]));
            
            Assert.True(result.IsSuccess);
            var value = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, value.TypeHint);
            Assert.Equal(32, value.Contents.Count);
            Assert.Equal(1337, value.Contents.AsSpan().I32);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void StoreInt32Local(int localIndex)
        {
            PrepareMethodWithLocal(localIndex, ModuleFixture.MockModule.CorLibTypeFactory.Int32);
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(new BitVector(1337), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Stloc, Context.CurrentFrame.Body!.LocalVariables[localIndex]));
            
            Assert.True(result.IsSuccess);

            var value = new BitVector(32, false).AsSpan();
            Context.CurrentFrame.ReadLocal(localIndex, value);
            
            Assert.Equal(1337, value.I32);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void PushInt8Local(int localIndex)
        {
            PrepareMethodWithLocal(localIndex, ModuleFixture.MockModule.CorLibTypeFactory.SByte);

            var expected = new BitVector(8, false);
            expected.AsSpan().Write((sbyte) -1);
            Context.CurrentFrame.WriteLocal(localIndex, expected.AsSpan());
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Ldloc, Context.CurrentFrame.Body!.LocalVariables[localIndex]));
            
            Assert.True(result.IsSuccess);
            var value = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, value.TypeHint);
            Assert.Equal(32, value.Contents.Count);
            Assert.Equal(-1, value.Contents.AsSpan().I32);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void PushUInt8Local(int localIndex)
        {
            PrepareMethodWithLocal(localIndex, ModuleFixture.MockModule.CorLibTypeFactory.Byte);

            var expected = new BitVector(8, false);
            expected.AsSpan().Write((sbyte) -1);
            Context.CurrentFrame.WriteLocal(localIndex, expected.AsSpan());
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Ldloc, Context.CurrentFrame.Body!.LocalVariables[localIndex]));
            
            Assert.True(result.IsSuccess);
            var value = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, value.TypeHint);
            Assert.Equal(32, value.Contents.Count);
            Assert.Equal(0xFF, value.Contents.AsSpan().I32);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void StoreUInt8Local(int localIndex)
        {
            PrepareMethodWithLocal(localIndex, ModuleFixture.MockModule.CorLibTypeFactory.Int32);
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(new BitVector(0x01234567), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Stloc, Context.CurrentFrame.Body!.LocalVariables[localIndex]));
            
            Assert.True(result.IsSuccess);

            var value = new BitVector(32, false).AsSpan();
            Context.CurrentFrame.ReadLocal(localIndex, value);
            
            Assert.Equal(0x01234567, value.I32);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void PushLocalAddress(int localIndex)
        {
            PrepareMethodWithLocal(localIndex, ModuleFixture.MockModule.CorLibTypeFactory.Int32);
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Ldloca, Context.CurrentFrame.Body!.LocalVariables[localIndex]));
            
            Assert.True(result.IsSuccess);

            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            Assert.Equal(
                Context.CurrentFrame.GetLocalAddress(localIndex),
                slot.Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit));
        }
    }
}