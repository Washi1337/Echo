using Echo.Code;
using Iced.Intel;

namespace Echo.Platforms.Iced
{
    /// <summary>
    /// Represents a variable represented by an x86 flag register. 
    /// </summary>
    public class X86FlagsRegister : IVariable
    {
        /// <summary>
        /// Creates a new x86 flag register variable.
        /// </summary>
        /// <param name="flag">The flag.</param>
        public X86FlagsRegister(RflagsBits flag)
        {
            Flag = flag;
        }

        /// <inheritdoc />
        public string Name => Flag.ToString();

        /// <summary>
        /// Gets the flag this variable is referencing.
        /// </summary>
        public RflagsBits Flag
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}