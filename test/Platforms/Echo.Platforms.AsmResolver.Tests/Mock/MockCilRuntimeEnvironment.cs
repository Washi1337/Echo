using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core.Code;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Tests.Mock
{
    public sealed class MockCilRuntimeEnvironment : ICilRuntimeEnvironment
    {
        public MockCilRuntimeEnvironment()
        {
            CliMarshaller = new DefaultCliMarshaller(this);
        }

        public IInstructionSetArchitecture<CilInstruction> Architecture
        {
            get;
            set;
        }

        public bool Is32Bit
        {
            get;
            set;
        }

        public ModuleDefinition Module
        {
            get;
            set;
        }

        public ICliMarshaller CliMarshaller
        {
            get;
            set;
        }

        public IMemoryAllocator MemoryAllocator
        {
            get;
            set;
        }

        public HookedMethodInvoker MethodInvoker
        {
            get;
            set;
        }

        IMethodInvoker ICilRuntimeEnvironment.MethodInvoker => this.MethodInvoker;

        public void Dispose()
        {
            MemoryAllocator?.Dispose();
        }
    }
}