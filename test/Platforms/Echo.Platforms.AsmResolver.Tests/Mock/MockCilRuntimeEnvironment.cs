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
            CliMarshaller = new DefaultCliMarshaller(this);
            ValueFactory = new DefaultValueFactory(module, is32Bit);
            StaticFieldFactory = new StaticFieldFactory(ValueFactory);
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
        public IValueFactory ValueFactory
        {
            get;
        }

        IMethodInvoker ICilRuntimeEnvironment.MethodInvoker => MethodInvoker;

        public void Dispose()
        {
            ValueFactory?.Dispose();
        }
    }
}