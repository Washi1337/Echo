using System;
using dnlib.DotNet.Emit;
using Echo.Core.Code;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Represents a variable that is declared and can be referenced within a CIL method body.
    /// </summary>
    public class CilVariable : IVariable
    {
        /// <summary>
        /// Creates a new CIL variable.
        /// </summary>
        /// <param name="variable">The underlying variable object.</param>
        public CilVariable(Local variable)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        }
        
        /// <summary>
        /// Gets the underlying local variable object.
        /// </summary>
        public Local Variable
        {
            get;
        }

        /// <inheritdoc />
        public string Name => $"V_{Variable.Index}";
        
        /// <inheritdoc />
        public override string ToString() => Name;
    }
}