using System;
using System.Collections.Generic;
using System.Text;
using AsmResolver.DotNet.Signatures;
using Echo.Concrete.Values.ReferenceType;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IMemoryAllocator"/> interface.
    /// </summary>
    public class DefaultMemoryAllocator : IMemoryAllocator
    {
        private readonly IDictionary<string, StringValue> _cachedStrings = new Dictionary<string, StringValue>();

        /// <summary>
        /// Creates a new instance of the <see cref="DefaultMemoryAllocator"/> class.
        /// </summary>
        /// <param name="is32Bit">Indicates the allocator is using 32 or 64 bit wide pointers.</param>
        public DefaultMemoryAllocator(bool is32Bit)
        {
            Is32Bit = is32Bit;
        }
        
        /// <summary>
        /// Gets a value indicating the memory allocator is using 32 or 64 bit wide pointers.
        /// </summary>
        public bool Is32Bit
        {
            get;
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

        /// <inheritdoc />
        public void Dispose()
        {
            // TODO: forced clean up of allocated memory (?)
        }
    }
}