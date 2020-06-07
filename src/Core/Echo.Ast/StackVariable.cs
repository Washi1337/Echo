using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Placeholder variable for replacing stack slots
    /// </summary>
    public sealed class StackVariable : IVariable
    {
        /// <summary>
        /// Creates a new placeholder variable for a stack slot
        /// </summary>
        /// <param name="name">The name of the variable</param>
        public StackVariable(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public string Name
        {
            get;
        }
    }
}