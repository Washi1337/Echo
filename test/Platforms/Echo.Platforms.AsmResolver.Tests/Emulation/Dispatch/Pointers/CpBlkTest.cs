using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class CpBlkTest : DispatcherTestBase
    {
        public CpBlkTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Fact]
        public void CpBlkKnownBytes()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;

            var stack = ExecutionContext.ProgramState.Stack;
            
            var sourceMemory = environment.MemoryAllocator.AllocateMemory(16, true);
            sourceMemory.WriteInteger64(0, new Integer64Value(0x0102030405060708));
            sourceMemory.WriteInteger64(8, new Integer64Value(0x090A0B0C0D0E0F00));
            
            var targetMemory = environment.MemoryAllocator.AllocateMemory(16, true);
            
            stack.Push(marshaller.ToCliValue(targetMemory, new PointerTypeSignature(environment.Module.CorLibTypeFactory.Int32)));
            stack.Push(marshaller.ToCliValue(sourceMemory, new PointerTypeSignature(environment.Module.CorLibTypeFactory.Int32)));
            stack.Push(new I4Value(8));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Cpblk));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0x0102030405060708ul, targetMemory.ReadInteger64(0).U64);
            Assert.Equal(0ul, targetMemory.ReadInteger64(8).U64);
        }

        [Fact]
        public void CpBlkPartiallyKnownBytes()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;

            var stack = ExecutionContext.ProgramState.Stack;
            
            var sourceMemory = environment.MemoryAllocator.AllocateMemory(16, true);
            sourceMemory.WriteInteger64(0,
                new Integer64Value("0011??000011??000011??000011??000011??000011??000011??000011??00"));
            sourceMemory.WriteInteger64(8, new Integer64Value(0x090A0B0C0D0E0F00));
            
            var targetMemory = environment.MemoryAllocator.AllocateMemory(16, true);
            
            stack.Push(marshaller.ToCliValue(targetMemory, new PointerTypeSignature(environment.Module.CorLibTypeFactory.Int32)));
            stack.Push(marshaller.ToCliValue(sourceMemory, new PointerTypeSignature(environment.Module.CorLibTypeFactory.Int32)));
            stack.Push(new I4Value(8));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Cpblk));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(
                new Integer64Value("0011??000011??000011??000011??000011??000011??000011??000011??00"), 
                targetMemory.ReadInteger64(0));
            Assert.Equal(0ul, targetMemory.ReadInteger64(8).U64);
        }
        
    }
}