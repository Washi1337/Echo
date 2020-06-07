using System.Collections.Generic;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an AST
    /// </summary>
    /// <remarks>This is not called 'Ast', because of the namespace</remarks>
    public sealed class AbstractSyntaxTree : AstNodeBase
    {
        /// <summary>
        /// Creates a new AST
        /// </summary>
        public AbstractSyntaxTree()
            : base(0) { }

        /// <summary>
        /// The tree's statements
        /// </summary>
        public IList<AstStatementBase> Statements
        {
            get;
        } = new List<AstStatementBase>();

        /// <inheritdoc />
        public override IEnumerable<AstNodeBase> GetChildren()
        {
            return Statements;
        }
    }
}