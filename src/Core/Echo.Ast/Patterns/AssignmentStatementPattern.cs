using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes an statement pattern that matches on an instance of a <see cref="AssignmentStatement{TInstruction}"/>. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class AssignmentStatementPattern<TInstruction> : StatementPattern<TInstruction>
    {
        /// <summary>
        /// Creates a new assignment statement pattern that matches on any variable and any value expression.
        /// </summary>
        public AssignmentStatementPattern()
        {
            Variables.Add(Pattern.Any<IVariable>());
            Expression = Pattern.Any<ExpressionBase<TInstruction>>();
        }
        
        /// <summary>
        /// Creates a new assignment statement pattern.
        /// </summary>
        /// <param name="variable">The pattern describing the variable that is assigned a value.</param>
        /// <param name="expression">The pattern of the expression placed on the right hand side of the equals sign.</param>
        public AssignmentStatementPattern(Pattern<IVariable> variable, Pattern<ExpressionBase<TInstruction>> expression)
        {
            Variables.Add(variable);
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        /// <summary>
        /// Creates a new assignment statement pattern.
        /// </summary>
        /// <param name="variables">The patterns describing the variables that is assigned a value.</param>
        /// <param name="expression">The pattern of the expression placed on the right hand side of the equals sign.</param>
        public AssignmentStatementPattern(IEnumerable<Pattern<IVariable>> variables, Pattern<ExpressionBase<TInstruction>> expression)
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
        public Pattern<ExpressionBase<TInstruction>> Expression
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void MatchChildren(StatementBase<TInstruction> input, MatchResult result)
        {
            if (!(input is AssignmentStatement<TInstruction> statement)
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

        /// <summary>
        /// Indicates the pattern should match on instances with the provided number of variables on the left hand side.
        /// </summary>
        /// <param name="variablesCount">The number of variables the assignment statement should have.</param>
        /// <returns>The current pattern.</returns>
        public AssignmentStatementPattern<TInstruction> WithVariables(int variablesCount)
        {
            Variables.Clear();

            for (int i = 0; i < variablesCount; i++)
                Variables.Add(Pattern.Any<IVariable>());
            
            return this;
        }

        /// <summary>
        /// Sets the patterns describing the variables that are assigned a new value.
        /// </summary>
        /// <param name="variables">The patterns describing the variables.</param>
        /// <returns>The current pattern.</returns>
        public AssignmentStatementPattern<TInstruction> WithVariables(params Pattern<IVariable>[] variables)
        {
            Variables.Clear();
            
            foreach (var variable in variables)
                Variables.Add(variable);
            
            return this;
        }

        /// <summary>
        /// Sets the pattern describing the expression on the right hand side of the equals sign.
        /// </summary>
        /// <param name="expression">The pattern describing expression.</param>
        /// <returns>The current pattern.</returns>
        public AssignmentStatementPattern<TInstruction> WithExpression(Pattern<ExpressionBase<TInstruction>> expression)
        {
            Expression = expression;
            return this;
        }

        /// <summary>
        /// Indicates all variables should be captured in a certain group.
        /// </summary>
        /// <param name="captureGroup">The group.</param>
        /// <returns>The current pattern.</returns>
        public AssignmentStatementPattern<TInstruction> CaptureVariables(CaptureGroup captureGroup)
        {
            foreach (var variable in Variables)
                variable.CaptureAs(captureGroup);

            return this;
        }

        /// <inheritdoc />
        public override string ToString() => $"{string.Join(", ", Variables)} = {Expression}";
    }
}