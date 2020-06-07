using System.Collections.Generic;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an assignment in an AST
    /// </summary>
    public sealed class AstAssignmentStatement : AstStatementBase
    {
        /// <summary>
        /// Creates a new assignment statement
        /// </summary>
        /// <param name="id"><inheritdoc cref="AstNodeBase(long)"/></param>
        /// <param name="variable">The variable</param>
        /// <param name="expression">The expression</param>
        public AstAssignmentStatement(long id, AstVariableExpression variable, AstExpressionBase expression)
            : base(id)
        {
            Variable = variable;
            Expression = expression;
        }

        /// <summary>
        /// The variable that this assignment targets
        /// </summary>
        public AstVariableExpression Variable
        {
            get;
        }

        /// <summary>
        /// The expression that is assigned to the target variable
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
                Variable,
                Expression
            };
        }
    }
}