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
    public class CpObjTest : DispatcherTestBase
    {
        public CpObjTest(MockModuleFixture moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void CopyFromKnownAddressShouldWriteContents()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            
            var int32Type = environment.Module.CorLibTypeFactory.Int32;
            var intPointerType = new PointerTypeSignature(int32Type);
            
            var sourcePointer = environment.ValueFactory.AllocateMemory(sizeof(int), false);
            sourcePointer.WriteInteger32(0, new Integer32Value(0x12340000, 0xFFFF0000));
            
            var destinationPointer = environment.ValueFactory.AllocateMemory(sizeof(int) * 2, false);
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(environment.CliMarshaller.ToCliValue(destinationPointer, intPointerType));
            stack.Push(environment.CliMarshaller.ToCliValue(sourcePointer, intPointerType));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Cpobj, int32Type.Type));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(0x12340000, 0xFFFF0000), destinationPointer.ReadInteger32(0));
            Assert.False(destinationPointer.ReadInteger32(sizeof(int)).IsKnown);
        }

        [Fact]
        public void CopyFromUnknownAddressShouldSetToUnknownBits()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            
            var int32Type = environment.Module.CorLibTypeFactory.Int32;
            var intPointerType = new PointerTypeSignature(int32Type);
            
            var destinationPointer = environment.ValueFactory.AllocateMemory(sizeof(int), true);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(environment.CliMarshaller.ToCliValue(destinationPointer, intPointerType));
            stack.Push(new PointerValue(false));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Cpobj, int32Type.Type));
            
            Assert.True(result.IsSuccess);
            Assert.False(destinationPointer.ReadInteger32(0).IsKnown);
        }
    }
}