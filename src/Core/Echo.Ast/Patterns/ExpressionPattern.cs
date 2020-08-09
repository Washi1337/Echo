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
        public static AnyPattern<AstExpressionBase<TInstruction>> Any<TInstruction>()
        {
            return new AnyPattern<AstExpressionBase<TInstruction>>();
        } 
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstInstructionExpression{TInstruction}"/>. 
        /// </summary>
        /// <param name="instruction">The instruction to match on.</param>
        /// <returns>The pattern.</returns>
        public static InstructionExpressionPattern<TInstruction> InstructionLiteral<TInstruction>(TInstruction instruction)
        {
            return new InstructionExpressionPattern<TInstruction>(Pattern.Literal(instruction));
        }
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstInstructionExpression{TInstruction}"/>. 
        /// </summary>
        /// <param name="instruction">The instruction pattern to match on.</param>
        /// <returns>The pattern.</returns>
        public static InstructionExpressionPattern<TInstruction> Instruction<TInstruction>(Pattern<TInstruction> instruction)
        {
            return new InstructionExpressionPattern<TInstruction>(instruction);
        }

        /// <summary>
        /// Creates a new pattern that matches any type of variable expression. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public static VariableExpressionPattern<TInstruction> Variable<TInstruction>()
        {
            return new VariableExpressionPattern<TInstruction>();
        }
    }
    
    /// <summary>
    /// Describes a pattern for an expression in an abstract syntax tree.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the abstract syntax tree.</typeparam>
    public abstract class ExpressionPattern<TInstruction> : Pattern<AstExpressionBase<TInstruction>>
    {
    }
}