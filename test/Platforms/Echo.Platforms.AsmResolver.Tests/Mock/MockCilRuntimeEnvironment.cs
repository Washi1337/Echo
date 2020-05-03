using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ReferenceType;
using Echo.Core.Code;
using Echo.Platforms.AsmResolver.Emulation;
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

        public MemoryPointerValue AllocateMemory(int size, bool initializeWithZeroes)
        {
            var memory = new Memory<byte>(new byte[size]);
            var knownBitMask = new Memory<byte>(new byte[size]);
            if (initializeWithZeroes)
                knownBitMask.Span.Fill(0xFF);
            return new MemoryPointerValue(memory, knownBitMask, Is32Bit);
        }

        public IDotNetArrayValue AllocateArray(TypeSignature elementType, int length)
        {
            if (elementType.IsValueType)
            {
                int size = length * elementType.GetSize(Is32Bit);
                var memory = AllocateMemory(size, true);
                return new ValueTypeArrayValue(elementType, memory);
            }
            
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}