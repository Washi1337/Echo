using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes an statement pattern that matches on an instance of a <see cref="AstAssignmentStatement{TInstruction}"/>. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class AssignmentStatementPattern<TInstruction> : StatementPattern<TInstruction>
    {
        /// <summary>
        /// Creates a new assignment statement pattern.
        /// </summary>
        /// <param name="variable">The pattern describing the variable that is assigned a value.</param>
        /// <param name="expression">The pattern of the expression placed on the right hand side of the equals sign.</param>
        public AssignmentStatementPattern(Pattern<IVariable> variable, Pattern<AstExpressionBase<TInstruction>> expression)
        {
            Variables.Add(variable);
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        /// <summary>
        /// Creates a new assignment statement pattern.
        /// </summary>
        /// <param name="variables">The patterns describing the variables that is assigned a value.</param>
        /// <param name="expression">The pattern of the expression placed on the right hand side of the equals sign.</param>
        public AssignmentStatementPattern(IEnumerable<Pattern<IVariable>> variables, Pattern<AstExpressionBase<TInstruction>> expression)
        {
            foreach (var variable in variables)
                Variables.Add(variable);
            Expression = expression;
        }

        /// <summary>
        /// Gets a collection of patterns describing the variables that are assigned a value.
        /// </summary>
        public IList<Pattern<IVariable>> Variables
        {
            get;
        } = new List<Pattern<IVariable>>();
        
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
            if (!(input is AstAssignmentStatement<TInstruction> statement)
                || statement.Variables.Length != Variables.Count)
            {
                result.IsSuccess = false;
                return;
            }

            for (int i = 0; i < Variables.Count; i++)
            {
                Variables[i].Match(statement.Variables[i], result);
                if (!result.IsSuccess)
                    return;
            }

            Expression.Match(statement.Expression, result);
        }

        /// <inheritdoc />
        public override string ToString() => $"{string.Join(", ", Variables)} = {Expression}";
    }
}