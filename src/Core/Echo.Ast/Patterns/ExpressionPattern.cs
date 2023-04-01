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
        public static AnyPattern<Expression<TInstruction>> Any<TInstruction>() => 
            new AnyPattern<Expression<TInstruction>>();

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="InstructionExpression{TInstruction}"/>. 
        /// </summary>
        /// <param name="instruction">The instruction to match on.</param>
        /// <returns>The pattern.</returns>
        public static InstructionExpressionPattern<TInstruction> InstructionLiteral<TInstruction>(TInstruction instruction) => 
            new InstructionExpressionPattern<TInstruction>(Pattern.Literal(instruction));

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="InstructionExpression{TInstruction}"/>. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public static InstructionExpressionPattern<TInstruction> Instruction<TInstruction>() => 
            new InstructionExpressionPattern<TInstruction>(Pattern.Any<TInstruction>());

        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="InstructionExpression{TInstruction}"/>. 
        /// </summary>
        /// <param name="instruction">The instruction pattern to match on.</param>
        /// <returns>The pattern.</returns>
        public static InstructionExpressionPattern<TInstruction> Instruction<TInstruction>(Pattern<TInstruction> instruction) => 
            new InstructionExpressionPattern<TInstruction>(instruction);

        /// <summary>
        /// Creates a new pattern that matches any type of variable expression. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public static VariableExpressionPattern<TInstruction> Variable<TInstruction>() => 
            new VariableExpressionPattern<TInstruction>();

        /// <summary>
        /// Creates a new pattern that matches any type of variable expression. 
        /// </summary>
        /// <param name="variable">The pattern describing the referenced variable.</param>
        /// <returns>The pattern.</returns>
        public static VariableExpressionPattern<TInstruction> Variable<TInstruction>(Pattern<IVariable> variable) => 
            new VariableExpressionPattern<TInstruction>(variable);
    }
    
    /// <summary>
    /// Describes a pattern for an expression in an abstract syntax tree.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the abstract syntax tree.</typeparam>
    public abstract class ExpressionPattern<TInstruction> : Pattern<Expression<TInstruction>>
    {
    }
}