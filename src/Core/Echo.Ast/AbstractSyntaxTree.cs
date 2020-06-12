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
        public AbstractSyntaxTree(long id = 0)
            : base(id) { }

        /// <summary>
        /// Nested trees
        /// </summary>
        public IList<AbstractSyntaxTree> Nested
        {
            get;
        } = new List<AbstractSyntaxTree>();

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
            foreach (var stmt in Statements)
                yield return stmt;
            
            foreach (var nest in Nested)
                yield return nest;
        }
    }
}
