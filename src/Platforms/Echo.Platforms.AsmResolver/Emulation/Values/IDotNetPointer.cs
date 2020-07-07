using System;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a pointer value that obeys the .NET type system.
    /// </summary>
    public interface IDotNetPointer : IPointerValue
    {
        /// <summary>
        /// Reads a single .NET structure at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start reading.</param>
        /// <param name="type">The structure type to read.</param>
        /// <returns>The read structure.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        IConcreteValue ReadStruct(int offset, TypeSignature type);

        /// <summary>
        /// Writes a single .NET structure at the provided offset.
        /// </summary>
        /// <param name="offset">The offset to start writing at.</param>
        /// <param name="type">The structure type to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the offset does not fall within the memory range.
        /// </exception>
        void WriteStruct(int offset, TypeSignature type, IConcreteValue value);
    }
}