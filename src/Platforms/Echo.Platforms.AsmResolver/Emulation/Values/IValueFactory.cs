
using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides factory members for constructing values by type. 
    /// </summary>
    public interface IValueFactory : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether a single pointer returned by this value factory is 32-bits or 64-bits wide.  
        /// </summary>
        bool Is32Bit
        {
            get;
        }

        /// <summary>
        /// Creates a value for the provided type that is optionally initialized with zeroes. 
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="initialize">Indicates whether the bits in the created object should be initialized to zero.</param>
        /// <returns>The default value.</returns>
        IConcreteValue CreateValue(TypeSignature type, bool initialize);

        /// <summary>
        /// Creates an object reference to a value for the provided type that is optionally initialized with zeroes. 
        /// </summary>
        /// <param name="type">The type.</param>
        /// /// <param name="initialize">Indicates whether the bits in the created object should be initialized to zero.</param>
        /// <returns>The default value.</returns>
        ObjectReference CreateObject(TypeSignature type, bool initialize);
        
        /// <summary>
        /// Allocates a chunk of addressable memory on the virtual heap, and returns a pointer value to the start of
        /// the memory chunk.  
        /// </summary>
        /// <param name="size">The size of the region to allocate.</param>
        /// <param name="initialize">Indicates the memory region should be initialized with zeroes.</param>
        /// <returns>A pointer to the memory.</returns>
        MemoryPointerValue AllocateMemory(int size, bool initialize);

        /// <summary>
        /// Allocates an array on the virtual heap.
        /// </summary>
        /// <param name="elementType">The type of elements to store in the array.</param>
        /// <param name="length">The number of elements.</param>
        /// <returns>The array.</returns>
        IDotNetArrayValue AllocateArray(TypeSignature elementType, int length);

        /// <summary>
        /// Allocates a structure.
        /// </summary>
        /// <param name="type">The type of object to allocate.</param>
        /// <param name="initialize">Indicates the memory region should be initialized with zeroes.</param>
        /// <returns>The allocated object.</returns>
        IDotNetStructValue AllocateStruct(TypeSignature type, bool initialize);

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