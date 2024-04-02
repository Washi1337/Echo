using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class NewObjHandlerTest : CilOpCodeHandlerTestBase
    {
        public NewObjHandlerTest(MockModuleFixture fixture)
            : base(fixture)
        {
            Context.Machine.Invoker = DefaultInvokers.ReturnUnknown;
        }

        [Fact]
        public void NewObject()
        {
            // Find System.Object::.ctor()
            var type = ModuleFixture.MockModule.CorLibTypeFactory.Object.Resolve()!;
            var constructor = type.Methods.First(m => m is {IsConstructor: true, Parameters.Count: 0});
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Newobj, constructor));
            
            // Verify that we jumped into the base method implementation.
            Assert.True(result.IsSuccess);
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            var instanceType = slot.Contents.AsObjectHandle(Context.Machine).GetMethodTable().Type;
            Assert.Equal("System.Object", instanceType.FullName);
        }

        [Fact]
        public void NewClassShouldInitializeFieldsToZero()
        {
            // Find SimpleClass::.ctor()
            var type = ModuleFixture.MockModule.LookupMember<TypeDefinition>(typeof(SimpleClass).MetadataToken);
            var constructor = type.Methods.First(m => m is {IsConstructor: true, Parameters.Count: 0});
            
            var result = Dispatcher.Dispatch(Context, new CilInstruction(CilOpCodes.Newobj, constructor));
            
            // Verify that we jumped into the base method implementation.
            Assert.True(result.IsSuccess);
            
            var slot = Context.CurrentFrame.EvaluationStack.Peek();
            Assert.Equal(StackSlotTypeHint.Integer, slot.TypeHint);
            
            var handle = slot.Contents.AsObjectHandle(Context.Machine);
            Assert.Equal(typeof(SimpleClass).FullName, handle.GetMethodTable().Type.FullName);

            Assert.All(type.Fields.Where(f=>!f.IsStatic), field =>
            {
                Assert.True(handle.ReadField(field).AsSpan().IsZero.ToBoolean());                
            });
        }
    }
}