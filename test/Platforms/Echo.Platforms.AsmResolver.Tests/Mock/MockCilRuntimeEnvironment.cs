using System;
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
        public MockCilRuntimeEnvironment(ModuleDefinition module, bool is32Bit)
        {
            Is32Bit = is32Bit;
            Module = module ?? throw new ArgumentNullException(nameof(module));
            MemoryAllocator = new DefaultMemoryAllocator(module, is32Bit);
            CliMarshaller = new DefaultCliMarshaller(this);
            UnknownValueFactory = new UnknownValueFactory(this);
            StaticFieldFactory = new StaticFieldFactory(UnknownValueFactory, MemoryAllocator);
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

        public StaticFieldFactory StaticFieldFactory
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IUnknownValueFactory UnknownValueFactory
        {
            get;
        }

        IMethodInvoker ICilRuntimeEnvironment.MethodInvoker => MethodInvoker;

        public void Dispose()
        {
            MemoryAllocator?.Dispose();
        }
    }
}