using System.Collections.Generic;
using Echo.Code;

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
        public static AnyPattern<Statement<TInstruction>> Any<TInstruction>()
            where TInstruction : notnull
            => new();

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AssignmentStatement{TInstruction}"/> with
        /// a single variable on the left hand side of the equals sign.
        /// </summary>
        public static AssignmentStatementPattern<TInstruction> Assignment<TInstruction>() 
            where TInstruction : notnull
            => new(Pattern.Any<IVariable>(), Pattern.Any<Expression<TInstruction>>());

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AssignmentStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="variable">The pattern describing the variable that is assigned a value.</param>
        /// <param name="expression">The pattern for the expression on the right hand side of the equals sign.</param>
        public static AssignmentStatementPattern<TInstruction> Assignment<TInstruction>(
            Pattern<IVariable> variable,
            Pattern<Expression<TInstruction>> expression)
            where TInstruction : notnull
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
            Pattern<Expression<TInstruction>> expression)
            where TInstruction : notnull
        {
            return new AssignmentStatementPattern<TInstruction>(variables, expression);
        }

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="ExpressionStatement{TInstruction}"/> with
        /// any kind of embedded expression.
        /// </summary>
        public static ExpressionStatementPattern<TInstruction> Expression<TInstruction>() 
            where TInstruction : notnull
            => new();

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="ExpressionStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="expression">The pattern for the embedded expression.</param>
        public static ExpressionStatementPattern<TInstruction> Expression<TInstruction>(
            Pattern<Expression<TInstruction>> expression)
            where TInstruction : notnull
        {
            return new ExpressionStatementPattern<TInstruction>(expression);
        }

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="ExpressionStatement{TInstruction}"/> that
        /// contain instances of <see cref="InstructionExpression{TInstruction}"/>.
        /// </summary>
        /// <param name="instruction">The instruction pattern to match on.</param>
        /// <returns>The pattern.</returns>
        public static ExpressionStatementPattern<TInstruction> Instruction<TInstruction>(
            Pattern<TInstruction> instruction)
            where TInstruction : notnull
        {
            return new ExpressionStatementPattern<TInstruction>(ExpressionPattern.Instruction(instruction));
        }

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="PhiStatementPattern{TInstruction}"/> with
        /// any target and any number of source variables.
        /// </summary>
        /// <returns>The pattern.</returns>
        public static PhiStatementPattern<TInstruction> Phi<TInstruction>() 
            where TInstruction : notnull
            => new();

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="PhiStatementPattern{TInstruction}"/> with
        /// any number of source variables.
        /// </summary>
        /// <param name="target">The target pattern to match on.</param>
        /// <returns>The pattern.</returns>
        public static PhiStatementPattern<TInstruction> Phi<TInstruction>(Pattern<IVariable> target) 
            where TInstruction : notnull
            => new(target);
    }
    
    /// <summary>
    /// Describes a pattern for a statement in an abstract syntax tree.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the abstract syntax tree.</typeparam>
    public abstract class StatementPattern<TInstruction> : Pattern<Statement<TInstruction>>
        where TInstruction : notnull
    {
    }
}