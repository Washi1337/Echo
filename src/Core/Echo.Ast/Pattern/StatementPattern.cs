namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Describes a pattern for a statement in an abstract syntax tree.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the abstract syntax tree.</typeparam>
    public abstract class StatementPattern<TInstruction> : Pattern<AstStatementBase<TInstruction>>
    {
        /// <summary>
        /// Creates a new pattern that matches any type of statement. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public new static AnyPattern<AstStatementBase<TInstruction>> Any()
        {
            return new AnyPattern<AstStatementBase<TInstruction>>();
        } 
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstAssignmentStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="expression">The pattern for the expression on the right hand side of the equals sign.</param>
        public static AssignmentStatementPattern<TInstruction> Assignment(ExpressionPattern<TInstruction> expression)
        {
            return new AssignmentStatementPattern<TInstruction>(expression);
        }
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstExpressionStatement{TInstruction}"/>.
        /// </summary>
        /// <param name="expression">The pattern for the embedded expression.</param>
        public static ExpressionStatementPattern<TInstruction> Expression(ExpressionPattern<TInstruction> expression)
        {
            return new ExpressionStatementPattern<TInstruction>(expression);
        }
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstExpressionStatement{TInstruction}"/> that
        /// contain instances of <see cref="AstInstructionExpression{TInstruction}"/>.
        /// </summary>
        /// <param name="instruction">The instruction to match on.</param>
        /// <returns>The pattern.</returns>
        public static ExpressionStatementPattern<TInstruction> Instruction(TInstruction instruction)
        {
            return new ExpressionStatementPattern<TInstruction>(ExpressionPattern<TInstruction>.Instruction(instruction));
        }
        
        /// <summary>
        /// Creates a new pattern that matches on instances of <see cref="AstExpressionStatement{TInstruction}"/> that
        /// contain instances of <see cref="AstInstructionExpression{TInstruction}"/>.
        /// </summary>
        /// <param name="instruction">The instruction pattern to match on.</param>
        /// <returns>The pattern.</returns>
        public static ExpressionStatementPattern<TInstruction> Instruction(Pattern<TInstruction> instruction)
        {
            return new ExpressionStatementPattern<TInstruction>(ExpressionPattern<TInstruction>.Instruction(instruction));
        }
    }
}