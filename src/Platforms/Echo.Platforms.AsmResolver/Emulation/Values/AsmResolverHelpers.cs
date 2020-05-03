using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides extension methods to the AsmResolver API.
    /// </summary>
    public static class AsmResolverHelpers
    {
        /// <summary>
        /// Computes the raw size of a type signature.
        /// </summary>
        /// <param name="type">The type to get the size of.</param>
        /// <param name="is32Bit">Indicates pointers are 32 or 64 bits wide.</param>
        /// <returns>The size in bytes.</returns>
        public static int GetSize(this TypeSignature type, bool is32Bit)
        {
            // TODO: replace length calculation with future AsmResolver GetSize methods.
            
            if (type.Namespace != "System")
                throw new NotSupportedException();
            
            return type.Name switch
            {
                nameof(Boolean) => 4,
                nameof(Char) => 2, 
                nameof(SByte) => 1,
                nameof(Byte) => 1,
                nameof(Int16) => 2,
                nameof(UInt16) => 2,
                nameof(Int32) => 4,
                nameof(UInt32) => 4,
                nameof(Int64) => 8,
                nameof(UInt64) => 8,
                nameof(Single) => 8,
                nameof(Double) => 8,
                nameof(IntPtr) => is32Bit ? 4 : 8,
                nameof(UIntPtr) => is32Bit ? 4 : 8,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}