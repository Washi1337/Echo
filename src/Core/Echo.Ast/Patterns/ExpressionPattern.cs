using Echo.Code;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Provides factory methods for constructing expression patterns.
    /// </summary>
    public static class ExpressionPattern
    {
        /// <summary>
        /// Creates a new pattern that matches any type of expressions. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public static AnyPattern<Expression<TInstruction>> Any<TInstruction>() 
            where TInstruction : notnull
            => new();

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="InstructionExpression{TInstruction}"/>. 
        /// </summary>
        /// <param name="instruction">The instruction to match on.</param>
        /// <returns>The pattern.</returns>
        public static InstructionExpressionPattern<TInstruction> InstructionLiteral<TInstruction>(TInstruction instruction)
            where TInstruction : notnull
            => new(Pattern.Literal(instruction));

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="InstructionExpression{TInstruction}"/>. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public static InstructionExpressionPattern<TInstruction> Instruction<TInstruction>() 
            where TInstruction : notnull
            => new(Pattern.Any<TInstruction>());

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="InstructionExpression{TInstruction}"/>. 
        /// </summary>
        /// <param name="instruction">The instruction pattern to match on.</param>
        /// <returns>The pattern.</returns>
        public static InstructionExpressionPattern<TInstruction> Instruction<TInstruction>(Pattern<TInstruction> instruction) 
            where TInstruction : notnull
            => new(instruction);

        /// <summary>
        /// Creates a new pattern that matches any type of variable expression. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public static VariableExpressionPattern<TInstruction> Variable<TInstruction>() 
            where TInstruction : notnull
            => new();

        /// <summary>
        /// Creates a new pattern that matches any type of variable expression. 
        /// </summary>
        /// <param name="variable">The pattern describing the referenced variable.</param>
        /// <returns>The pattern.</returns>
        public static VariableExpressionPattern<TInstruction> Variable<TInstruction>(Pattern<IVariable> variable) 
            where TInstruction : notnull
            => new(variable);
    }
    
    /// <summary>
    /// Describes a pattern for an expression in an abstract syntax tree.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the abstract syntax tree.</typeparam>
    public abstract class ExpressionPattern<TInstruction> : Pattern<Expression<TInstruction>>
        where TInstruction : notnull
    {
        /// <summary>
        /// Wraps the expression pattern in an expression statement pattern.
        /// </summary>
        /// <returns>The resulting statement pattern.</returns>
        public ExpressionStatementPattern<TInstruction> ToStatement() => new(this);
    }
}