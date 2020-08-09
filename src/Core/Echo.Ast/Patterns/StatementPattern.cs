using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Provides factory methods for constructing statement patterns.
    /// </summary>
    public static class StatementPattern
    {
        /// <summary>
        /// Creates a new pattern that matches any type of statement. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public static AnyPattern<StatementBase<TInstruction>> Any<TInstruction>()
        {
            return new AnyPattern<StatementBase<TInstruction>>();
        }

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AssignmentStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="variable">The pattern describing the variable that is assigned a value.</param>
        /// <param name="expression">The pattern for the expression on the right hand side of the equals sign.</param>
        public static AssignmentStatementPattern<TInstruction> Assignment<TInstruction>(
            Pattern<IVariable> variable,
            Pattern<ExpressionBase<TInstruction>> expression)
        {
            return new AssignmentStatementPattern<TInstruction>(variable, expression);
        }

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AssignmentStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="variables">The patterns describing the variables that is assigned a value.</param>
        /// <param name="expression">The pattern for the expression on the right hand side of the equals sign.</param>
        public static AssignmentStatementPattern<TInstruction> Assignment<TInstruction>(
            IEnumerable<Pattern<IVariable>> variables,
            Pattern<ExpressionBase<TInstruction>> expression)
        {
            return new AssignmentStatementPattern<TInstruction>(variables, expression);
        }

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="ExpressionStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="expression">The pattern for the embedded expression.</param>
        public static ExpressionStatementPattern<TInstruction> Expression<TInstruction>(Pattern<ExpressionBase<TInstruction>> expression)
        {
            return new ExpressionStatementPattern<TInstruction>(expression);
        }

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="ExpressionStatement{TInstruction}"/> that
        /// contain instances of <see cref="InstructionExpression{TInstruction}"/>.
        /// </summary>
        /// <param name="instruction">The instruction to match on.</param>
        /// <returns>The pattern.</returns>
        public static ExpressionStatementPattern<TInstruction> Instruction<TInstruction>(TInstruction instruction)
        {
            return new ExpressionStatementPattern<TInstruction>(ExpressionPattern.InstructionLiteral(instruction));
        }

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="ExpressionStatement{TInstruction}"/> that
        /// contain instances of <see cref="InstructionExpression{TInstruction}"/>.
        /// </summary>
        /// <param name="instruction">The instruction pattern to match on.</param>
        /// <returns>The pattern.</returns>
        public static ExpressionStatementPattern<TInstruction> Instruction<TInstruction>(Pattern<TInstruction> instruction)
        {
            return new ExpressionStatementPattern<TInstruction>(ExpressionPattern.Instruction(instruction));
        }
    }
    
    /// <summary>
    /// Describes a pattern for a statement in an abstract syntax tree.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the abstract syntax tree.</typeparam>
    public abstract class StatementPattern<TInstruction> : Pattern<StatementBase<TInstruction>>
    {
    }
}