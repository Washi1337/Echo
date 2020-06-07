using System.Collections.Generic;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a statement with an expression in an AST
    /// </summary>
    public sealed class AstExpressionStatement : AstStatementBase
    {
        /// <summary>
        /// Creates a new expression statement
        /// </summary>
        /// <param name="id"><inheritdoc cref="AstNodeBase(long)"/></param>
        /// <param name="expression">The expression</param>
        public AstExpressionStatement(long id, AstExpressionBase expression)
            : base(id)
        {
            Expression = expression;
        }

        /// <summary>
        /// The expression of this statement
        /// </summary>
        public AstExpressionBase Expression
        {
            get;
        }

        /// <inheritdoc />
        public override IEnumerable<AstNodeBase> GetChildren()
        {
            return new[]
            {
                Expression
            };
        }
    }
}