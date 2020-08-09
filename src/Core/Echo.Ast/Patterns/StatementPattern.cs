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
        public new static AnyPattern<AstStatementBase<TInstruction>> Any<TInstruction>()
        {
            return new AnyPattern<AstStatementBase<TInstruction>>();
        } 
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstAssignmentStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="expression">The pattern for the expression on the right hand side of the equals sign.</param>
        public static AssignmentStatementPattern<TInstruction> Assignment<TInstruction>(ExpressionPattern<TInstruction> expression)
        {
            return new AssignmentStatementPattern<TInstruction>(expression);
        }
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstExpressionStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="expression">The pattern for the embedded expression.</param>
        public static ExpressionStatementPattern<TInstruction> Expression<TInstruction>(ExpressionPattern<TInstruction> expression)
        {
            return new ExpressionStatementPattern<TInstruction>(expression);
        }
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstExpressionStatement{TInstruction}"/> that
        /// contain instances of <see cref="AstInstructionExpression{TInstruction}"/>.
        /// </summary>
        /// <param name="instruction">The instruction to match on.</param>
        /// <returns>The pattern.</returns>
        public static ExpressionStatementPattern<TInstruction> Instruction<TInstruction>(TInstruction instruction)
        {
            return new ExpressionStatementPattern<TInstruction>(ExpressionPattern.InstructionLiteral(instruction));
        }
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstExpressionStatement{TInstruction}"/> that
        /// contain instances of <see cref="AstInstructionExpression{TInstruction}"/>.
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
    public abstract class StatementPattern<TInstruction> : Pattern<AstStatementBase<TInstruction>>
    {
    }
}