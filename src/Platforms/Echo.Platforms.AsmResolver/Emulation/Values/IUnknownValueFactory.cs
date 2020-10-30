
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
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
        IConcreteValue CreateUnknown(TypeSignature type);
    }
}