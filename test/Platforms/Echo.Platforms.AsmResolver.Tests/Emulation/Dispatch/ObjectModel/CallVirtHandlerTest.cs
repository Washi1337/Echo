using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class CallVirtHandlerTest: CilOpCodeHandlerTestBase
    {
        public CallVirtHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
            Context.Machine.Invoker = DefaultInvokers.StepIn;
        }

        [Fact]
        public void CallVirtInstanceShouldStepInOriginalMethodIfNoOverride()
        {
            // Find SimpleClass and a virtual method to call.
            var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.VirtualInstanceMethod));
            
            // Allocate an instance of the class, and push it onto the stack.
            long objectPointer = Context.Machine.Heap.AllocateObject(type, true);
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(
                Context.Machine.ValueFactory.CreateNativeInteger(objectPointer),
                StackSlotTypeHint.Integer));

            // Execute a callvirt.
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Callvirt, method));
            
            // Verify that we jumped into the base method implementation.
            Assert.True(result.IsSuccess);
            Assert.Same(method, Context.CurrentFrame.Method);
        }
        
        [Fact]
        public void CallVirtInstanceShouldStepInOverrideMethodIfAvailable()
        {
            // Find SimpleClass and a virtual method to call
            var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
            var baseMethod = type.Methods.First(m => m.Name == nameof(SimpleClass.VirtualInstanceMethod));
            
            // Find DerivedSimpleClass and and overridden method that is finally called.
            var derivedType = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(DerivedSimpleClass));
            var overriddenMethod = derivedType.Methods.First(m => m.Name == nameof(DerivedSimpleClass.VirtualInstanceMethod));
            
            // Allocate an instance of the class, and push it onto the stack.
            long objectPointer = Context.Machine.Heap.AllocateObject(derivedType, true);
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(
                Context.Machine.ValueFactory.CreateNativeInteger(objectPointer), 
                StackSlotTypeHint.Integer));

            // Execute a callvirt.
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Callvirt, baseMethod));
            
            // Verify that we jumped into the overridden method.
            Assert.True(result.IsSuccess);
            Assert.Same(overriddenMethod, Context.CurrentFrame.Method);
        }

        [Fact]
        public void CallVirtOnNullShouldThrow()
        {
            // Find SimpleClass and a virtual method to call
            var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
            var baseMethod = type.Methods.First(m => m.Name == nameof(SimpleClass.VirtualInstanceMethod));

            // Push "null"
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(
                Context.Machine.ValueFactory.CreateNull(), 
                StackSlotTypeHint.Integer));
            
            // Execute a callvirt.
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Callvirt, baseMethod));

            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer?.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
        }

        [Fact]
        public void CallVirtOnUnknownShouldDispatchToUnknownResolver()
        {
            var resolver = new MyUnknownResolver();
            Context.Machine.UnknownResolver = resolver;
            
            // Find SimpleClass and a virtual method to call
            var type = ModuleFixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(SimpleClass));
            var baseMethod = type.Methods.First(m => m.Name == nameof(SimpleClass.VirtualInstanceMethod));
            
            // Push "unknown"
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(
                Context.Machine.ValueFactory.CreateNativeInteger(false), 
                StackSlotTypeHint.Integer));
            
            // Execute a callvirt.
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Callvirt, baseMethod));

            Assert.True(result.IsSuccess);
            Assert.Equal(resolver.LastResolveMethodAttempt, baseMethod);
        }

        private sealed class MyUnknownResolver : ThrowUnknownResolver
        {
            public IMethodDescriptor? LastResolveMethodAttempt;
            
            public override IMethodDescriptor? ResolveMethod(CilExecutionContext context, CilInstruction instruction, IList<BitVector> arguments)
            {
                LastResolveMethodAttempt = (IMethodDescriptor) instruction.Operand!;
                return null;
            }
        }
    }
}