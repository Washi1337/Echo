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
            return type.ElementType switch
            {
                ElementType.Boolean => 4,
                ElementType.Char => 2,
                ElementType.I1 => 1,
                ElementType.U1 => 1,
                ElementType.I2 => 2,
                ElementType.U2 => 2,
                ElementType.I4 => 4,
                ElementType.U4 => 4,
                ElementType.I8 => 8,
                ElementType.U8 => 8,
                ElementType.R4 => 4,
                ElementType.R8 => 8,
                ElementType.I => is32Bit ? 4 : 8,
                ElementType.U => is32Bit ? 4 : 8,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}