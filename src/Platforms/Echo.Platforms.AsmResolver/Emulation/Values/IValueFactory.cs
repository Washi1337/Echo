
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides factory members for constructing values by type. 
    /// </summary>
    public interface IValueFactory
    {
        /// <summary>
        /// Creates an unknown value for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The unknown value.</returns>
        IConcreteValue CreateUnknown(TypeSignature type);
    }
}