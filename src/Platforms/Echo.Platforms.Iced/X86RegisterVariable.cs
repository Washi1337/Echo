using Echo.Core.Code;
using Iced.Intel;

namespace Echo.Platforms.Iced
{
    /// <summary>
    /// Represents a variable represented by an x86 register. 
    /// </summary>
    public class X86RegisterVariable : IVariable
    {
        /// <summary>
        /// Creates a new x86 register variable.
        /// </summary>
        /// <param name="register">The register.</param>
        public X86RegisterVariable(Register register)
        {
            Register = register;
        }
        
        /// <inheritdoc />
        public string Name => Register.ToString();

        /// <summary>
        /// Gets the register this variable is referencing.
        /// </summary>
        public Register Register
        {
            get;
        }
        
    }
}