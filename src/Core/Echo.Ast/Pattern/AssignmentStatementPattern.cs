using System;

namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Describes an statement pattern that matches on an instance of a <see cref="AstAssignmentStatement{TInstruction}"/>. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class AssignmentStatementPattern<TInstruction> : Pattern<TInstruction>
    {
        /// <summary>
        /// Creates a new assignment statement pattern.
        /// </summary>
        /// <param name="expression">The pattern of the expression placed on the right hand side of the equals sign.</param>
        public AssignmentStatementPattern(Pattern<AstExpressionBase<TInstruction>> expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }
        
        /// <summary>
        /// Gets or sets the pattern describing the expression embedded into the input.
        /// </summary>
        public Pattern<AstExpressionBase<TInstruction>> Expression
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void MatchChildren(TInstruction input, MatchResult result)
        {
            if (!(input is AstAssignmentStatement<TInstruction> statement))
            {
                result.IsSuccess = false;
                return;
            }

            Expression.Match(statement.Expression, result);
        }

        /// <inheritdoc />
        public override string ToString() => $"var = {Expression}";
    }
}