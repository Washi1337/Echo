using System;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes an statement pattern that matches on an instance of a <see cref="ExpressionStatement{TInstruction}"/>. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class ExpressionStatementPattern<TInstruction> : StatementPattern<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new expression statement pattern matching on any embedded expression.
        /// </summary>
        public ExpressionStatementPattern()
        {
            Expression = Pattern.Any<Expression<TInstruction>>();
        }
        
        /// <summary>
        /// Creates a new expression statement pattern.
        /// </summary>
        /// <param name="expression">The pattern for the embedded expression.</param>
        public ExpressionStatementPattern(Pattern<Expression<TInstruction>> expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }
        
        /// <summary>
        /// Gets or sets the pattern describing the expression embedded into the input.
        /// </summary>
        public Pattern<Expression<TInstruction>> Expression
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void MatchChildren(Statement<TInstruction> input, MatchResult result)
        {
            if (!(input is ExpressionStatement<TInstruction> statement))
            {
                result.IsSuccess = false;
                return;
            }

            Expression.Match(statement.Expression, result);
        }

        /// <summary>
        /// Sets the pattern describing the expression embedded into the input.
        /// </summary>
        /// <param name="expression">The expression pattern.</param>
        /// <returns>The current pattern.</returns>
        public ExpressionStatementPattern<TInstruction> WithExpression(Pattern<Expression<TInstruction>> expression)
        {
            Expression = expression;
            return this;
        }
        
        /// <inheritdoc />
        public override string ToString() => Expression.ToString();
    }
}