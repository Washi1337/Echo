
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides members for describing a value in a managed .NET environment.
    /// </summary>
    public interface IDotNetValue : IConcreteValue
    {
        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        TypeSignature Type
        {
            get;
        }
    }
}