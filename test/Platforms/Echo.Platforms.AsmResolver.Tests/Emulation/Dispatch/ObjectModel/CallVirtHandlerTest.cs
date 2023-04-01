using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
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
                objectPointer,
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
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(objectPointer, StackSlotTypeHint.Integer));

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
            Context.CurrentFrame.EvaluationStack.Push(new StackSlot(0ul, StackSlotTypeHint.Integer));
            
            // Execute a callvirt.
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Callvirt, baseMethod));

            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer?.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.NullReferenceException", exceptionType?.FullName);
        }
    }
}