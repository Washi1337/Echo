
using AsmResolver.DotNet.Signatures.Types;

namespace Echo.Platforms.AsmResolver.Emulation.Values.Cli
{
    /// <summary>
    /// Provides factory members for creating unknown values by type. 
    /// </summary>
    public interface IUnknownValueFactory
    {
        /// <summary>
        /// Creates an unknown value for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The unknown value.</returns>
        ICliValue CreateUnknown(TypeSignature type);
    }
}