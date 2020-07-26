using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class LdindTest : DispatcherTestBase
    {
        public LdindTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Theory]
        [InlineData(0, 0x12345678)]
        [InlineData(4, 0x09ABCDEF)]
        public void LdindI4Test(int offset, int expected)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var factory = environment.Module.CorLibTypeFactory;
            
            var stack = ExecutionContext.ProgramState.Stack;

            var memory = environment.MemoryAllocator.AllocateMemory(8, true);
            memory.WriteInteger32(0, new Integer32Value(0x12345678));
            memory.WriteInteger32(4, new Integer32Value(0x09ABCDEF));
            
            var relativePointer = new RelativePointerValue(memory, offset);
            stack.Push(environment.CliMarshaller.ToCliValue(relativePointer, new PointerTypeSignature(factory.Byte)));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldind_I4));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(expected), stack.Top);
        }
    }
}