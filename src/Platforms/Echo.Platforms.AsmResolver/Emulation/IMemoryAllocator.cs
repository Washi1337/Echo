using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values.ReferenceType;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides methods for allocating blocks of memory in a .NET virtual machine.  
    /// </summary>
    public interface IMemoryAllocator : IDisposable
    {
        /// <summary>
        /// Allocates a chunk of addressable memory on the virtual heap, and returns a pointer value to the start of
        /// the memory chunk.  
        /// </summary>
        /// <param name="size">The size of the region to allocate.</param>
        /// <param name="initializeWithZeroes">Indicates the memory region should be initialized with zeroes.</param>
        /// <returns>A pointer to the memory.</returns>
        MemoryPointerValue AllocateMemory(int size, bool initializeWithZeroes);

        /// <summary>
        /// Allocates an array on the virtual heap.
        /// </summary>
        /// <param name="elementType">The type of elements to store in the array.</param>
        /// <param name="length">The number of elements.</param>
        /// <returns>The array.</returns>
        IDotNetArrayValue AllocateArray(TypeSignature elementType, int length);

        IDotNetObjectValue AllocateObject(TypeSignature type);

        /// <summary>
        /// Gets the string value for the fully known string literal.
        /// </summary>
        /// <param name="value">The string literal.</param>
        /// <returns>The string value.</returns>
        StringValue GetStringValue(string value);

        /// <summary>
        /// Gets the raw memory layout of a type within the virtual machine.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The memory layout.</returns>
        TypeMemoryLayout GetTypeMemoryLayout(ITypeDescriptor type);
    }
}