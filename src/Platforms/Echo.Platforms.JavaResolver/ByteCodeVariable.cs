using System;
using Echo.Core.Code;
using JavaResolver.Class.Code;

namespace Echo.Platforms.JavaResolver
{
    /// <summary>
    ///     Represents a variable.
    /// </summary>
    public class ByteCodeVariable : IVariable
    {
        /// <summary>
        ///     Creates a new variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public ByteCodeVariable(LocalVariable variable) => Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        
        /// <summary>
        ///     Gets the variable.
        /// </summary>
        public LocalVariable Variable { get; }

        /// <inheritdoc />
        public string Name => Variable.Name;

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}