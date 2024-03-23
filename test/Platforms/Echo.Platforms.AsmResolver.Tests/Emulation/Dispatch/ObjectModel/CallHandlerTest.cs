using System;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
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
        public void CallStaticUsingStepOver()
        {
            var invoker = new InvokerWrapper(DefaultInvokers.ReturnUnknown);
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(ModuleFixture.MockModule.CorLibTypeFactory.Int32));

            Context.Machine.Invoker = invoker;

            var callerFrame = Context.CurrentFrame;
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.Equal(InvocationResultType.StepOver, invoker.LastInvocationResult.ResultType);
            
            Assert.Same(callerFrame, Context.CurrentFrame);
            Assert.Equal(0x0005, callerFrame.ProgramCounter);
            Assert.Same(method, invoker.LastMethod);

            var returnValue = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, returnValue.TypeHint);
            Assert.False(returnValue.Contents.AsSpan().IsFullyKnown);
        }

        [Fact]
        public void CallStaticWithParametersUsingStepOver()
        {
            var invoker = new InvokerWrapper(DefaultInvokers.ReturnUnknown);
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("Dummy", MethodAttributes.Static, MethodSignature.CreateStatic(
                factory.Int32,
                factory.Int32,
                factory.Int32,
                factory.Int32));

            Context.Machine.Invoker = invoker;

            var callerFrame = Context.CurrentFrame;
            callerFrame.EvaluationStack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(1, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(2, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.Equal(InvocationResultType.StepOver, invoker.LastInvocationResult.ResultType);
            Assert.Single(callerFrame.EvaluationStack);
            Assert.Same(method, invoker.LastMethod);
            Assert.Equal(new[] {0, 1, 2}, invoker.LastArguments!.Select(x => x.AsSpan().I32));
        }

        [Fact]
        public void CallInstanceWithParametersUsingStepOver()
        {
            var invoker = new InvokerWrapper(DefaultInvokers.ReturnUnknown);
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("Dummy", MethodAttributes.Public, MethodSignature.CreateInstance(
                factory.Int32,
                factory.Int32,
                factory.Int32,
                factory.Int32));

            Context.Machine.Invoker = invoker;

            var callerFrame = Context.CurrentFrame;
            callerFrame.EvaluationStack.Push(new StackSlot(0x12345678, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(1, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(2, StackSlotTypeHint.Integer));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.Equal(InvocationResultType.StepOver, invoker.LastInvocationResult.ResultType);
            Assert.Single(callerFrame.EvaluationStack);
            Assert.Same(method, invoker.LastMethod);
            Assert.Equal(new[] {0x12345678, 0, 1, 2}, invoker.LastArguments!.Select(x => x.AsSpan().I32));
        }

        [Fact]
        public void CallStaticUsingStepIn()
        {  
            var invoker = new InvokerWrapper(DefaultInvokers.StepIn);
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(ModuleFixture.MockModule.CorLibTypeFactory.Int32));

            Context.Machine.Invoker = invoker;
            
            var callerFrame = Context.CurrentFrame;

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(InvocationResultType.StepIn, invoker.LastInvocationResult.ResultType);
            
            Assert.NotSame(callerFrame, Context.CurrentFrame);
            Assert.Equal(method, Context.CurrentFrame.Method);
            Assert.Equal(0x0005, callerFrame.ProgramCounter);
        }
        
        [Fact]
        public void CallStaticWithParametersUsingStepIn()
        {  
            var invoker = new InvokerWrapper(DefaultInvokers.StepIn);
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("Dummy", MethodAttributes.Static, MethodSignature.CreateStatic(
                factory.Int32,
                factory.Int32,
                factory.Int32,
                factory.Int32));

            Context.Machine.Invoker = invoker;
            
            var callerFrame = Context.CurrentFrame;
            callerFrame.EvaluationStack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(1, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(2, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(InvocationResultType.StepIn, invoker.LastInvocationResult.ResultType);
            
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
            var frame = Context.Thread.CallStack.Push(method);
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

        [Fact]
        public void CallShouldMarshalArgumentsUsingCorrectTypes()
        {
            var invoker = new InvokerWrapper(DefaultInvokers.ReturnUnknown);
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("Dummy", MethodAttributes.Static, MethodSignature.CreateStatic(
                factory.Int32, 
                factory.Int32,
                factory.Double,
                factory.Double));

            Context.Machine.Invoker = invoker;

            var callerFrame = Context.CurrentFrame;
            callerFrame.EvaluationStack.Push(new StackSlot(0, StackSlotTypeHint.Integer));
            callerFrame.EvaluationStack.Push(new StackSlot(1D, StackSlotTypeHint.Float));
            callerFrame.EvaluationStack.Push(new StackSlot(2D, StackSlotTypeHint.Float));
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.Equal(InvocationResultType.StepOver, invoker.LastInvocationResult.ResultType);
            Assert.Single(callerFrame.EvaluationStack);
            Assert.Same(method, invoker.LastMethod);

            var arguments = invoker.LastArguments;
            Assert.Equal(3, arguments!.Count);
            
            Assert.Equal(0, arguments[0].AsSpan().I32);
            Assert.Equal(1D, arguments[1].AsSpan().F64);
            Assert.Equal(2D, arguments[2].AsSpan().F64);
        }

        [Fact]
        public void CallMethodWithGenericArguments()
        {
            var invoker = new InvokerWrapper(DefaultInvokers.ReturnUnknown);
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("Dummy", MethodAttributes.Static, MethodSignature.CreateStatic(
                    factory.Int32,
                    1,
                    new GenericParameterSignature(GenericParameterType.Method, 0)))
                .MakeGenericInstanceMethod(factory.Int32);

            Context.Machine.Invoker = invoker;

            var callerFrame = Context.CurrentFrame;
            callerFrame.EvaluationStack.Push(new StackSlot(1, StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.Single(callerFrame.EvaluationStack);
            Assert.Same(method, invoker.LastMethod);
            
            Assert.Single(invoker.LastArguments!);
            Assert.Equal(1, invoker.LastArguments![0].AsSpan().I32);
        }

        [Fact]
        public void CallMethodWithGenericArgumentFromCaller()
        {
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;
            
            var caller = new MethodDefinition(
                "GenericCaller",
                MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Int32, 1, new GenericParameterSignature(GenericParameterType.Method, 0))
            ).MakeGenericInstanceMethod(factory.Int32);

            var module = new ModuleDefinition("DummyModule");
            var calleeType = new TypeDefinition(null, "SomeGenericType", TypeAttributes.Class, factory.Object.Type);
            module.TopLevelTypes.Add(calleeType);
            
            var callee = calleeType
                .MakeGenericInstanceType(new GenericParameterSignature(GenericParameterType.Method, 0))
                .ToTypeDefOrRef()
                .CreateMemberReference("SomeMethod", MethodSignature.CreateStatic(factory.Void));
            
            Context.Thread.CallStack.Push(caller);
            Context.Machine.Invoker = DefaultInvokers.StepIn;
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, callee));

            Assert.Equal(CilDispatchResult.Success(), result);
            Assert.Equal(calleeType
                    .MakeGenericInstanceType(factory.Int32)
                    .ToTypeDefOrRef()
                    .CreateMemberReference(callee.Name!, MethodSignature.CreateStatic(factory.Void)),
                Context.CurrentFrame.Method,
                SignatureComparer.Default
            );
        }

        [Fact]
        public void CallStepInWithInitializer()
        {
            // Look up metadata.
            var type = ModuleFixture.MockModule.LookupMember<TypeDefinition>(typeof(ClassWithInitializer).MetadataToken);
            var cctor = type.GetStaticConstructor();
            var method = type.Methods.First(m => m.Name == nameof(ClassWithInitializer.MethodFieldAccess));

            // Step into method.
            Context.Machine.Invoker = DefaultInvokers.StepIn;
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            
            // Verify that the .cctor is called.
            Assert.Same(cctor, Context.Thread.CallStack.Peek(0).Method);
            Assert.Same(method, Context.Thread.CallStack.Peek(1).Method);
        }

        [Fact]
        public void CallStepInWithThrowingInitializer()
        {
            // Look up metadata.
            var type = ModuleFixture.MockModule.LookupMember<TypeDefinition>(typeof(ClassWithThrowingInitializer).MetadataToken);
            var cctor = type.GetStaticConstructor();
            var method = type.Methods.First(m => m.Name == nameof(ClassWithThrowingInitializer.MethodFieldAccess));

            // Step into method.
            Context.Machine.Invoker = DefaultInvokers.StepIn;
            Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, method));

            Assert.Same(cctor, Context.Thread.CallStack.Peek(0).Method);
            Assert.Same(method, Context.Thread.CallStack.Peek(1).Method);

            var exception = Assert.Throws<EmulatedException>(() => Context.Thread.StepOut());
            Assert.Equal(nameof(TypeInitializationException), exception.ExceptionObject.GetObjectType().Name);
        }

        [Fact]
        public void ConstrainedCallAbstractStatic()
        {
            var factory = ModuleFixture.MockModule.CorLibTypeFactory;

            // Set up dummy metadata.
            var module = new ModuleDefinition("Dummy");
            
            // static abstract IFoo::Method()
            var interfaceType = new TypeDefinition(null, "IFoo", TypeAttributes.Interface);
            var interfaceMethod = new MethodDefinition(
                "Method",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Abstract
                | MethodAttributes.Virtual | MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void)
            );
            interfaceType.Methods.Add(interfaceMethod);
            module.TopLevelTypes.Add(interfaceType);
            
            // static Foo::Method() that implements IFoo::Method()
            var classType = new TypeDefinition(null, "Foo", TypeAttributes.Interface);
            classType.Interfaces.Add(new InterfaceImplementation(interfaceType));
            var classMethod = new MethodDefinition(
                "Method",
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                MethodSignature.CreateStatic(factory.Void)
            );
            classType.MethodImplementations.Add(new MethodImplementation(interfaceMethod, classMethod));
            classType.Methods.Add(classMethod);
            module.TopLevelTypes.Add(classType);

            // Allocate a Foo object.
            long objectPointer = Context.Machine.Heap.AllocateObject(classType, true);
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(
                Context.Machine.ValueFactory.CreateNativeInteger(objectPointer),
                StackSlotTypeHint.Integer));
            
            // Execute a constrained callvirt.
            Context.Machine.Invoker = DefaultInvokers.StepIn;
            Context.CurrentFrame.ConstrainedType = classType;
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Call, interfaceMethod));
            
            // Verify we entered Foo::Method()
            Assert.Equal(CilDispatchResult.Success(), result);
            Assert.Equal(classMethod, Context.CurrentFrame.Method, SignatureComparer.Default);
        }
    }
}