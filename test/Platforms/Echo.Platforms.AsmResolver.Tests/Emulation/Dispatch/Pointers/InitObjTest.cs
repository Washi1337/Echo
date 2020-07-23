using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class InitObjTest : DispatcherTestBase
    {
        public InitObjTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void InitializePrimitiveObject()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            
            var int32Type = environment.Module.CorLibTypeFactory.Int32;
            
            var pointer = environment.MemoryAllocator.AllocateMemory(sizeof(int) * 2, false);
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(environment.CliMarshaller.ToCliValue(pointer, new PointerTypeSignature(int32Type)));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Initobj, int32Type.Type));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(0), pointer.ReadInteger32(0));
            Assert.False(pointer.ReadInteger32(sizeof(int)).IsKnown);
        }
    }
}