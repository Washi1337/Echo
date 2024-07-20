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
    public class ArgumentsTest : CilOpCodeHandlerTestBase
    {
        public ArgumentsTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }
        
        private void PrepareMethodWithArgument(int localCount, TypeSignature localType)
        {
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("DummyMethod", MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void));
            
            for (int i = 0; i <= localCount; i++)
                method.Signature!.ParameterTypes.Add(localType);
            method.Parameters.PullUpdatesFromMethodSignature();

            var body = new CilMethodBody(method);
            method.CilMethodBody = body;

            Context.Thread.CallStack.Push(method);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void PushInt32Argument(int argIndex)
        {
            PrepareMethodWithArgument(argIndex, ModuleFixture.MockModule.CorLibTypeFactory.Int32);
            Context.CurrentFrame.WriteArgument(argIndex, new BitVector(1337).AsSpan());
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Ldarg, Context.CurrentFrame.Body!.Owner.Parameters[argIndex]));
            
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
        public void StoreInt32Argument(int argIndex)
        {
            PrepareMethodWithArgument(argIndex, ModuleFixture.MockModule.CorLibTypeFactory.Int32);
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(new BitVector(1337), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Starg, Context.CurrentFrame.Body!.Owner.Parameters[argIndex]));
            
            Assert.True(result.IsSuccess);

            var value = new BitVector(32, false).AsSpan();
            Context.CurrentFrame.ReadArgument(argIndex, value);
            
            Assert.Equal(1337, value.I32);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void PushInt8Argument(int argIndex)
        {
            PrepareMethodWithArgument(argIndex, ModuleFixture.MockModule.CorLibTypeFactory.SByte);

            var expected = new BitVector(8, false);
            expected.AsSpan().Write((sbyte) -1);
            Context.CurrentFrame.WriteArgument(argIndex, expected.AsSpan());
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Ldarg, Context.CurrentFrame.Body!.Owner.Parameters[argIndex]));
            
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
        public void PushUInt8Argument(int argIndex)
        {
            PrepareMethodWithArgument(argIndex, ModuleFixture.MockModule.CorLibTypeFactory.Byte);

            var expected = new BitVector(8, false);
            expected.AsSpan().Write((sbyte) -1);
            Context.CurrentFrame.WriteArgument(argIndex, expected.AsSpan());
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Ldarg, Context.CurrentFrame.Body!.Owner.Parameters[argIndex]));
            
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
        public void StoreUInt8Argument(int argIndex)
        {
            PrepareMethodWithArgument(argIndex, ModuleFixture.MockModule.CorLibTypeFactory.Int32);
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(new BitVector(0x01234567), StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Starg, Context.CurrentFrame.Body!.Owner.Parameters[argIndex]));
            
            Assert.True(result.IsSuccess);

            var value = new BitVector(32, false).AsSpan();
            Context.CurrentFrame.ReadArgument(argIndex, value);
            
            Assert.Equal(0x01234567, value.I32);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void PushArgumentAddress(int argIndex)
        {
            PrepareMethodWithArgument(argIndex, ModuleFixture.MockModule.CorLibTypeFactory.Int32);
            
            var result = Dispatcher.Dispatch(Context, 
                new CilInstruction(CilOpCodes.Ldarga, Context.CurrentFrame.Body!.Owner.Parameters[argIndex]));
            
            Assert.True(result.IsSuccess);

            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            Assert.Equal(
                Context.CurrentFrame.GetArgumentAddress(argIndex),
                slot.Contents.AsSpan().ReadNativeInteger(Context.Machine.Is32Bit));
        }
    }
}