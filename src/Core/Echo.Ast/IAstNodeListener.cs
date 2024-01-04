namespace Echo.Ast
{
    /// <summary>
    /// Provides methods for entering and exiting nodes in an AST during a traversal by an
    /// <see cref="AstNodeWalker{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
    public interface IAstNodeListener<TInstruction>
    {
        /// <summary>
        /// Enters an assignment statement.
        /// </summary>
        /// <param name="statement">The statement to enter.</param>
        void EnterAssignmentStatement(AssignmentStatement<TInstruction> statement);
        
        /// <summary>
        /// Exits an assignment statement.
        /// </summary>
        /// <param name="statement">The statement to exit.</param>
        void ExitAssignmentStatement(AssignmentStatement<TInstruction> statement);
        
        /// <summary>
        /// Enters an expression statement.
        /// </summary>
        /// <param name="statement">The statement to enter.</param>
        void EnterExpressionStatement(ExpressionStatement<TInstruction> statement);
        
        /// <summary>
        /// Exits an expression statement.
        /// </summary>
        /// <param name="statement">The statement to exit.</param>
        void ExitExpressionStatement(ExpressionStatement<TInstruction> statement);
        
        /// <summary>
        /// Enters a PHI statement.
        /// </summary>
        /// <param name="statement">The statement to enter.</param>
        void EnterPhiStatement(PhiStatement<TInstruction> statement);
        
        /// <summary>
        /// Exits a PHI statement.
        /// </summary>
        /// <param name="statement">The statement to exit.</param>
        void ExitPhiStatement(PhiStatement<TInstruction> statement);
        
        /// <summary>
        /// Enters a variable expression.
        /// </summary>
        /// <param name="expression">The expression to enter.</param>
        void EnterVariableExpression(VariableExpression<TInstruction> expression);
        
        /// <summary>
        /// Exits a variable expression.
        /// </summary>
        /// <param name="expression">The expression to exit.</param>
        void ExitVariableExpression(VariableExpression<TInstruction> expression);
        
        /// <summary>
        /// Enters an instruction expression.
        /// </summary>
        /// <param name="expression">The expression to enter.</param>
        void EnterInstructionExpression(InstructionExpression<TInstruction> expression);
        
        /// <summary>
        /// Exits an instruction expression.
        /// </summary>
        /// <param name="expression">The expression to exit..</param>
        void ExitInstructionExpression(InstructionExpression<TInstruction> expression);
    }

}