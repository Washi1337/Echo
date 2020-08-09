using System;
using Echo.Core.Code;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes a pattern that matches on <see cref="AstVariableExpression{TInstruction}"/>.
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
        protected override void MatchChildren(AstExpressionBase<TInstruction> input, MatchResult result)
        {
            if (!(input is AstVariableExpression<TInstruction> expression))
            {
                result.IsSuccess = false;
                return;
            }

            Variable.Match(expression.Variable, result);
        }

        /// <inheritdoc />
        public override string ToString() => Variable.ToString();
        
    }
}