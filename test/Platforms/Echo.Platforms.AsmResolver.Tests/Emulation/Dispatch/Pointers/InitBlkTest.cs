using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class InitBlkTest : DispatcherTestBase
    {
        public InitBlkTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Fact]
        public void InitBlkWithKnownValuesTest()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;

            var stack = ExecutionContext.ProgramState.Stack;
            
            var memory = environment.MemoryAllocator.AllocateMemory(16, true);
            stack.Push(marshaller.ToCliValue(memory, new PointerTypeSignature(environment.Module.CorLibTypeFactory.Int32)));
            stack.Push(new I4Value(0x01));
            stack.Push(new I4Value(8));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Initblk));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0x01010101_01010101ul, memory.ReadInteger64(0).U64);
            Assert.Equal(0ul, memory.ReadInteger64(8).U64);
        }

        [Fact]
        public void InitBlkWithPartiallyKnownValuesTest()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;

            var stack = ExecutionContext.ProgramState.Stack;
            
            var memory = environment.MemoryAllocator.AllocateMemory(16, true);
            stack.Push(marshaller.ToCliValue(memory, new PointerTypeSignature(environment.Module.CorLibTypeFactory.Int32)));
            stack.Push(new I4Value("0011??00"));
            stack.Push(new I4Value(8));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Initblk));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(
                new Integer64Value("0011??000011??000011??000011??000011??000011??000011??000011??00"),
                memory.ReadInteger64(0));
            Assert.Equal(0ul, memory.ReadInteger64(8).U64);
        }
    }
}