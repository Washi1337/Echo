using System;
using System.Collections.Generic;
using System.Text;
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
        private readonly IDictionary<string, StringValue> _cachedStrings = new Dictionary<string,StringValue>();

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

        /// <inheritdoc />
        public MemoryPointerValue AllocateMemory(int size, bool initializeWithZeroes)
        {
            var memory = new Memory<byte>(new byte[size]);
            var knownBitMask = new Memory<byte>(new byte[size]);
            if (initializeWithZeroes)
                knownBitMask.Span.Fill(0xFF);
            return new MemoryPointerValue(memory, knownBitMask, Is32Bit);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public StringValue GetStringValue(string value)
        {
            if (!_cachedStrings.TryGetValue(value, out var stringValue))
            {
                var rawMemory = AllocateMemory(value.Length * 2, false);
                var span = new ReadOnlySpan<byte>(Encoding.Unicode.GetBytes(value));
                rawMemory.WriteBytes(0, span);
                stringValue = new StringValue(rawMemory);
                _cachedStrings.Add(value, stringValue);
            }

            return stringValue;
        }

        public void Dispose()
        {
        }
    }
}