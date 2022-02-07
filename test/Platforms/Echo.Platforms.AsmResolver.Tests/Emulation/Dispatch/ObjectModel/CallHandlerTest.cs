using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class CallHandlerTest : CilOpCodeHandlerTestBase
    {
        public CallHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void CallStaticUsingInvoke()
        {
            var invoker = new InvokerWrapper(ReturnUnknownInvoker.Instance);
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(ModuleFixture.MockModule.CorLibTypeFactory.Int32));

            Context.Machine.InvocationStrategy = AlwaysInvokeStrategy.Instance;
            Context.Machine.Invoker = invoker;

            var callerFrame = Context.CurrentFrame;
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.True(invoker.HasInvoked);
            
            Assert.Same(callerFrame, Context.CurrentFrame);
            Assert.Equal(0x0005, callerFrame.ProgramCounter);
            Assert.Same(method, invoker.LastMethod);

            var returnValue = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, returnValue.TypeHint);
            Assert.False(returnValue.Contents.AsSpan().IsFullyKnown);
        }

        [Fact]
        public void CallStaticWithParametersUsingInvoke()
        {
            var invoker = new InvokerWrapper(ReturnUnknownInvoker.Instance);
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("Dummy", MethodAttributes.Static, MethodSignature.CreateStatic(
                factory.Int32,
                factory.Int32,
                factory.Int32,
                factory.Int32));

            Context.Machine.InvocationStrategy = AlwaysInvokeStrategy.Instance;
            Context.Machine.Invoker = invoker;

            var callerFrame = Context.CurrentFrame;
            callerFrame.EvaluationStack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(1, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(2, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.True(invoker.HasInvoked);
            Assert.Single(callerFrame.EvaluationStack);
            Assert.Same(method, invoker.LastMethod);
            Assert.Equal(new[] {0, 1, 2}, invoker.LastArguments.Select(x => x.AsSpan().I32));
        }

        [Fact]
        public void CallInstanceWithParametersUsingInvoke()
        {
            var invoker = new InvokerWrapper(ReturnUnknownInvoker.Instance);
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("Dummy", MethodAttributes.Public, MethodSignature.CreateInstance(
                factory.Int32,
                factory.Int32,
                factory.Int32,
                factory.Int32));

            Context.Machine.InvocationStrategy = AlwaysInvokeStrategy.Instance;
            Context.Machine.Invoker = invoker;

            var callerFrame = Context.CurrentFrame;
            callerFrame.EvaluationStack.Push(new StackSlot(0x12345678, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(1, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(2, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.True(invoker.HasInvoked);
            Assert.Single(callerFrame.EvaluationStack);
            Assert.Same(method, invoker.LastMethod);
            Assert.Equal(new[] {0x12345678, 0, 1, 2}, invoker.LastArguments.Select(x => x.AsSpan().I32));
        }

        [Fact]
        public void CallStaticUsingStepIn()
        {  
            var invoker = new InvokerWrapper(ReturnUnknownInvoker.Instance);
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(ModuleFixture.MockModule.CorLibTypeFactory.Int32));

            Context.Machine.InvocationStrategy = NeverInvokeStrategy.Instance;
            Context.Machine.Invoker = invoker;
            
            var callerFrame = Context.CurrentFrame;

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));
            
            Assert.True(result.IsSuccess);
            Assert.False(invoker.HasInvoked);
            
            Assert.NotSame(callerFrame, Context.CurrentFrame);
            Assert.Equal(method, Context.CurrentFrame.Method);
            Assert.Equal(0x0005, callerFrame.ProgramCounter);
        }
        
        [Fact]
        public void CallStaticWithParametersUsingStepIn()
        {  
            var invoker = new InvokerWrapper(ReturnUnknownInvoker.Instance);
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("Dummy", MethodAttributes.Static, MethodSignature.CreateStatic(
                factory.Int32,
                factory.Int32,
                factory.Int32,
                factory.Int32));

            Context.Machine.InvocationStrategy = NeverInvokeStrategy.Instance;
            Context.Machine.Invoker = invoker;
            
            var callerFrame = Context.CurrentFrame;
            callerFrame.EvaluationStack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(1, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(2, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));
            
            Assert.True(result.IsSuccess);
            Assert.False(invoker.HasInvoked);
            
            var calleeFrame = Context.CurrentFrame;
            Assert.NotSame(callerFrame, calleeFrame);
            Assert.Equal(method, calleeFrame.Method);

            var buffer = new BitVector(32, false).AsSpan();
            calleeFrame.ReadArgument(0, buffer);
            Assert.Equal(0, buffer.I32);
            calleeFrame.ReadArgument(1, buffer);
            Assert.Equal(1, buffer.I32);
            calleeFrame.ReadArgument(2, buffer);
            Assert.Equal(2, buffer.I32);
        }

        [Fact]
        public void ReturnShouldPushValueOntoStack()
        {
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(ModuleFixture.MockModule.CorLibTypeFactory.Int32));
            var frame = Context.Machine.CallStack.Push(method);
            frame.EvaluationStack.Push(new StackSlot(0x1337, StackSlotTypeHint.Integer));
            
            var calleeFrame = Context.CurrentFrame;
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Ret));
            
            Assert.True(result.IsSuccess);
            Assert.NotSame(calleeFrame, Context.CurrentFrame);

            var value = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, value.TypeHint);
            Assert.Equal(0x1337, value.Contents.AsSpan().I32);
            Assert.Equal(32, value.Contents.Count);
        }

        private class InvokerWrapper : IMethodInvoker
        {
            private readonly IMethodInvoker _invoker;

            public InvokerWrapper(IMethodInvoker invoker)
            {
                _invoker = invoker;
            }

            public bool HasInvoked
            {
                get;
                private set;
            }

            public IMethodDescriptor? LastMethod
            {
                get;
                private set;
            }

            public IList<BitVector>? LastArguments
            {
                get;
                private set;
            }

            public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
            {
                HasInvoked = true;
                LastMethod = method;
                LastArguments = arguments.Select(x => x.Clone()).ToArray();
                return _invoker.Invoke(context, method, arguments);
            }
        } 
    }
    
}