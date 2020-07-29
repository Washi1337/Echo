namespace Echo.Ast
{
    /// <summary>
    /// Provides a visitor interface to implement a visitor pattern on the AST
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction the AST models</typeparam>
    /// <typeparam name="TState">The state to pass between visitors</typeparam>
    public interface IAstNodeVisitor<TInstruction, in TState>
    {
        /// <summary>
        /// Visits a given <see cref="AstAssignmentStatement{TInstruction}"/>
        /// </summary>
        void Visit(AstAssignmentStatement<TInstruction> assignmentStatement, TState state);

        /// <summary>
        /// Visits a given <see cref="AstExpressionStatement{TInstruction}"/>
        /// </summary>
        void Visit(AstExpressionStatement<TInstruction> expressionStatement, TState state);
        
        /// <summary>
        /// Visits a given <see cref="AstInstructionExpression{TInstruction}"/>
        /// </summary>
        void Visit(AstInstructionExpression<TInstruction> instructionExpression, TState state);

        /// <summary>
        /// Visits a given <see cref="AstVariableExpression{TInstruction}"/>
        /// </summary>
        void Visit(AstVariableExpression<TInstruction> variableExpression, TState state);
    }

    /// <summary>
    /// Provides a visitor interface to implement a visitor pattern on the AST
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction the AST models</typeparam>
    /// <typeparam name="TState">The state to pass between visitors</typeparam>
    /// <typeparam name="TOut">The return type of the Visit methods</typeparam>
    public interface IAstNodeVisitor<TInstruction, in TState, out TOut>
    {
        /// <summary>
        /// Visits a given <see cref="AstAssignmentStatement{TInstruction}"/>
        /// </summary>
        TOut Visit(AstAssignmentStatement<TInstruction> assignmentStatement, TState state);

        /// <summary>
        /// Visits a given <see cref="AstExpressionStatement{TInstruction}"/>
        /// </summary>
        TOut Visit(AstExpressionStatement<TInstruction> expressionStatement, TState state);
        
        /// <summary>
        /// Visits a given <see cref="AstInstructionExpression{TInstruction}"/>
        /// </summary>
        TOut Visit(AstInstructionExpression<TInstruction> instructionExpression, TState state);

        /// <summary>
        /// Visits a given <see cref="AstVariableExpression{TInstruction}"/>
        /// </summary>
        TOut Visit(AstVariableExpression<TInstruction> variableExpression, TState state);
    }
}