
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides factory members for constructing values by type. 
    /// </summary>
    public interface IValueFactory
    {
        /// <summary>
        /// Creates a value for the provided type that is initialized with the default contents.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The default value.</returns>
        IConcreteValue CreateDefault(TypeSignature type);

        /// <summary>
        /// Creates an object reference to a value for the provided type that is initialized with the default contents.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The default value.</returns>
        ObjectReference CreateDefaultObject(TypeSignature type);

        /// <summary>
        /// Creates an unknown value for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The unknown value.</returns>
        IConcreteValue CreateUnknown(TypeSignature type);
        
        /// <summary>
        /// Creates an object reference to an unknown value for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The unknown value.</returns>
        ObjectReference CreateUnknownObject(TypeSignature type); 
    }
}