using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ReferenceType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class StIndTest : DispatcherTestBase
    {
        public StIndTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Theory]
        [InlineData(0, 0x12345678)]
        [InlineData(4, 0x09ABCDEF)]
        public void StindI4Test(int offset, int expected)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var factory = environment.Module.CorLibTypeFactory;
            
            var stack = ExecutionContext.ProgramState.Stack;

            var memory = environment.ValueFactory.AllocateMemory(8, true);
            
            var relativePointer = new RelativePointerValue(memory, offset, environment.Is32Bit);
            stack.Push(environment.CliMarshaller.ToCliValue(relativePointer, new PointerTypeSignature(factory.Byte)));
            stack.Push(new I4Value(expected));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Stind_I4));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(expected), memory.ReadInteger32(offset));
        }
    }
}