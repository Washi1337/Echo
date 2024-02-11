using System;
using Echo.Code;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes a pattern that matches on <see cref="VariableExpression{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class VariableExpressionPattern<TInstruction> : ExpressionPattern<TInstruction>
    {
        /// <summary>
        /// Creates a new variable expression pattern that matches any variable.
        /// </summary>
        public VariableExpressionPattern()
        {
            Variable = Pattern.Any<IVariable>();
        }

        /// <summary>
        /// Creates a new variable expression pattern.
        /// </summary>
        /// <param name="variable">The pattern describing the referenced variable.</param>
        public VariableExpressionPattern(Pattern<IVariable> variable)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        }

        /// <summary>
        /// Gets or sets a pattern describing the referenced variable. 
        /// </summary>
        public Pattern<IVariable> Variable
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        protected override void MatchChildren(Expression<TInstruction> input, MatchResult result)
        {
            if (!(input is VariableExpression<TInstruction> expression))
            {
                result.IsSuccess = false;
                return;
            }

            Variable.Match(expression.Variable, result);
        }

        /// <inheritdoc />
        public override string ToString() => Variable.ToString();

        /// <summary>
        /// Captures the embedded variable.
        /// </summary>
        /// <param name="captureGroup">The capture group to add the extracted variable to.</param>
        /// <returns>The current pattern.</returns>
        public Pattern<Expression<TInstruction>> CaptureVariable(CaptureGroup<IVariable> captureGroup)
        {
            Variable.CaptureAs(captureGroup);
            return this;
        }
    }
}