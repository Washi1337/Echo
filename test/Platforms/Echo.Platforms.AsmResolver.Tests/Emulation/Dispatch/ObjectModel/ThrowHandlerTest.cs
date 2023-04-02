using System.IO;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class ThrowHandlerTest : CilOpCodeHandlerTestBase
    {
        public ThrowHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void ThrowNullShouldThrowNullReference()
        {
            var machine = Context.Machine;
            var stack = Context.CurrentFrame.EvaluationStack;

            stack.Push(new StackSlot(machine.ValueFactory.CreateNativeInteger(0), StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Throw));
            
            Assert.False(result.IsSuccess);
            var exceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal("System.NullReferenceException", exceptionType.FullName);
        }

        [Fact]
        public void ThrowShouldThrowExceptionOnStack()
        {
            var machine = Context.Machine;
            var stack = Context.CurrentFrame.EvaluationStack;

            var exceptionType = ModuleDefinition.FromFile(typeof(IOException).Assembly.Location)
                .TopLevelTypes.First(t => t.IsTypeOf("System.IO", "IOException"))
                .ImportWith(Context.Machine.ContextModule.DefaultImporter);
            
            long exceptionPointer = machine.Heap.AllocateObject(exceptionType, true);
            stack.Push(new StackSlot(
                machine.ValueFactory.CreateNativeInteger(exceptionPointer), 
                StackSlotTypeHint.Integer));

            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Throw));
            
            Assert.False(result.IsSuccess);
            var observedExceptionType = result.ExceptionPointer!.AsObjectHandle(Context.Machine).GetObjectType();
            Assert.Equal(exceptionType, observedExceptionType, SignatureComparer.Default);
        }
    }
}