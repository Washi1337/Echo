using Echo.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a variable in the AST
    /// </summary>
    public sealed class AstVariable : IVariable
    {
        /// <summary>
        /// Creates a new variable with the given <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name to give to the variable</param>
        public AstVariable(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public string Name
        {
            get;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is AstVariable other))
                return false;

            return Name == other.Name;
        }

        /// <inheritdoc />
        public override int GetHashCode() => Name.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}