using AsmResolver.DotNet.Signatures;
using Echo.Concrete.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides members for marshalling concrete values put into variables and fields within a .NET program to
    /// values on the evaluation stack of the Common Language Infrastructure (CLI) and vice versa. 
    /// </summary>
    public interface ICliMarshaller
    {
        /// <summary>
        /// Gets a value indicating this marshaller assumes a 32 bit or a 64 bit architecture.
        /// </summary>
        bool Is32Bit
        {
            get;
        }
        
        /// <summary>
        /// Wraps a concrete value into a CLI value.
        /// </summary>
        /// <param name="value">The value to marshal.</param>
        /// <param name="originalType">The original type of the value as it is stored in a variable or field.</param>
        /// <returns>The CLI value.</returns>
        ICliValue ToCliValue(IConcreteValue value, TypeSignature originalType);

        /// <summary>
        /// Unwraps the CLI value into a concrete value that can be stored in a variable or field.
        /// </summary>
        /// <param name="value">The CLI value to unpack.</param>
        /// <param name="targetType">The target type to marshal the value to.</param>
        /// <returns>The unpacked value.</returns>
        IConcreteValue ToCtsValue(ICliValue value, TypeSignature targetType);
    }
}