using System;

namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Describes an statement pattern that matches on an instance of a <see cref="AstExpressionStatement{TInstruction}"/>. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class ExpressionStatementPattern<TInstruction> : StatementPattern<TInstruction>
    {
        /// <summary>
        /// Creates a new expression statement pattern.
        /// </summary>
        /// <param name="expression">The pattern for the embedded expression.</param>
        public ExpressionStatementPattern(Pattern<AstExpressionBase<TInstruction>> expression)
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
        protected override void MatchChildren(AstStatementBase<TInstruction> input, MatchResult result)
        {
            if (!(input is AstExpressionStatement<TInstruction> statement))
            {
                result.IsSuccess = false;
                return;
            }

            Expression.Match(statement.Expression, result);
        }
        
    }
}