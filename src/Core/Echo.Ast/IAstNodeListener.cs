namespace Echo.Ast
{
    /// <summary>
    /// Provides methods for entering and exiting nodes in an AST during a traversal by an
    /// <see cref="AstNodeWalker{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
    public interface IAstNodeListener<TInstruction>
    where TInstruction : notnull
    {
        /// <summary>
        /// Enters a compilation unit.
        /// </summary>
        /// <param name="unit">The unit to enter.</param>
        void EnterCompilationUnit(CompilationUnit<TInstruction> unit);

        /// <summary>
        /// Exits a compilation unit.
        /// </summary>
        /// <param name="unit">The unit to exit.</param>
        void ExitCompilationUnit(CompilationUnit<TInstruction> unit);
        
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
        /// Enters a block statement.
        /// </summary>
        /// <param name="statement">The statement to enter.</param>
        void EnterBlockStatement(BlockStatement<TInstruction> statement);

        /// <summary>
        /// Exits a block statement.
        /// </summary>
        /// <param name="statement">The statement to exit.</param>
        void ExitBlockStatement(BlockStatement<TInstruction> statement);

        /// <summary>
        /// Enters an exception handler statement.
        /// </summary>
        /// <param name="statement">The statement to enter.</param>
        void EnterExceptionHandlerStatement(ExceptionHandlerStatement<TInstruction> statement);

        /// <summary>
        /// Exits an exception handler statement.
        /// </summary>
        /// <param name="statement">The statement to exit.</param>
        void ExitExceptionHandlerBlock(ExceptionHandlerStatement<TInstruction> statement);

        /// <summary>
        /// Enters the protected block of an exception handler statement.
        /// </summary>
        /// <param name="statement">The parent exception handler statement.</param>
        void EnterProtectedBlock(ExceptionHandlerStatement<TInstruction> statement);

        /// <summary>
        /// Exits the protected block of an exception handler statement.
        /// </summary>
        /// <param name="statement">The parent exception handler statement.</param>
        void ExitProtectedBlock(ExceptionHandlerStatement<TInstruction> statement);

        /// <summary>
        /// Enters a handler clause of an exception handler statement.
        /// </summary>
        /// <param name="statement">The parent exception handler statement.</param>
        /// <param name="handlerIndex">The index of the handler clause to enter.</param>
        void EnterHandlerBlock(ExceptionHandlerStatement<TInstruction> statement, int handlerIndex);

        /// <summary>
        /// Exits a handler clause of an exception handler statement.
        /// </summary>
        /// <param name="statement">The parent exception handler statement.</param>
        /// <param name="handlerIndex">The index of the handler clause to exit.</param>
        void ExitHandlerBlock(ExceptionHandlerStatement<TInstruction> statement, int handlerIndex);

        /// <summary>
        /// Enters the prologue of a handler clause.
        /// </summary>
        /// <param name="clause">The parent clause.</param>
        void EnterPrologueBlock(HandlerClause<TInstruction> clause);

        /// <summary>
        /// Exits the prologue of a handler clause.
        /// </summary>
        /// <param name="clause">The parent clause.</param>
        void ExitPrologueBlock(HandlerClause<TInstruction> clause);

        /// <summary>
        /// Enters the epilogue of a handler clause.
        /// </summary>
        /// <param name="clause">The parent clause.</param>
        void EnterEpilogueBlock(HandlerClause<TInstruction> clause);

        /// <summary>
        /// Exits the epilogue of a handler clause.
        /// </summary>
        /// <param name="clause">The parent clause.</param>
        void ExitEpilogueBlock(HandlerClause<TInstruction> clause);

        /// <summary>
        /// Enters the main code of a handler clause.
        /// </summary>
        /// <param name="clause">The parent clause.</param>
        void EnterHandlerContents(HandlerClause<TInstruction> clause);

        /// <summary>
        /// Exits the main code of a handler clause.
        /// </summary>
        /// <param name="clause">The parent clause.</param>
        void ExitHandlerContents(HandlerClause<TInstruction> clause);
        
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