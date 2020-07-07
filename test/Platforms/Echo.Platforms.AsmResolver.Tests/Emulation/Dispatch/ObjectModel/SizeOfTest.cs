using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class SizeOfTest : DispatcherTestBase
    {
        public SizeOfTest(MockModuleProvider moduleProvider)
            : base(moduleProvider) { }

        [Fact]
        public void DispatchInt32()
        {
            var module = ModuleDefinition.FromFile(typeof(Test).Assembly.Location);
            var targetType = module.CorLibTypeFactory.Int32.ToTypeDefOrRef();
            var instruction = new CilInstruction(CilOpCodes.Sizeof, targetType);

            Dispatch(instruction, targetType);
        }

        [Fact]
        public void DispatchSingle()
        {
            var module = ModuleDefinition.FromFile(typeof(Test).Assembly.Location);
            var targetType = module.CorLibTypeFactory.Single.ToTypeDefOrRef();
            var instruction = new CilInstruction(CilOpCodes.Sizeof, targetType);

            Dispatch(instruction, targetType);
        }

        [Fact]
        public void DispatchStruct()
        {
            var module = ModuleDefinition.FromFile(typeof(Test).Assembly.Location);
            var targetType = (TypeDefinition) module.LookupMember(typeof(Test).MetadataToken);
            var instruction = new CilInstruction(CilOpCodes.Sizeof, targetType);

            Dispatch(instruction, targetType);
        }

        private void Dispatch(CilInstruction instruction, ITypeDescriptor targetType)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var allocator = environment.MemoryAllocator;
            var layout = allocator.GetTypeMemoryLayout(targetType);
            var value = new I4Value((int) layout.Size);

            var result = Dispatcher.Execute(ExecutionContext, instruction);

            Assert.True(result.IsSuccess);
            Assert.Equal(value, ExecutionContext.ProgramState.Stack.Top);
        }

        private struct Test
        {
            int Dummy1;

            long Dummy2;

            List<int> Dumm3;
        }
    }
}
