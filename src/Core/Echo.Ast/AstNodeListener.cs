namespace Echo.Ast
{
    /// <summary>
    /// Provides a base implementation for an <see cref="IAstNodeListener{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
    public abstract class AstNodeListener<TInstruction> : IAstNodeListener<TInstruction>
    {
        /// <inheritdoc />
        public void EnterCompilationUnit(CompilationUnit<TInstruction> unit) {}

        /// <inheritdoc />
        public void ExitCompilationUnit(CompilationUnit<TInstruction> unit) {}

        /// <inheritdoc />
        public virtual void EnterAssignmentStatement(AssignmentStatement<TInstruction> statement) {}
        
        /// <inheritdoc />
        public virtual void ExitAssignmentStatement(AssignmentStatement<TInstruction> statement) {}
        
        /// <inheritdoc />
        public virtual void EnterExpressionStatement(ExpressionStatement<TInstruction> statement) {}
        
        /// <inheritdoc />
        public virtual void ExitExpressionStatement(ExpressionStatement<TInstruction> statement) {}
        
        /// <inheritdoc />
        public virtual void EnterPhiStatement(PhiStatement<TInstruction> statement) {}
        
        /// <inheritdoc />
        public virtual void ExitPhiStatement(PhiStatement<TInstruction> statement) {}

        /// <inheritdoc />
        public virtual void EnterBlockStatement(BlockStatement<TInstruction> statement) {}

        /// <inheritdoc />
        public virtual void ExitBlockStatement(BlockStatement<TInstruction> statement) {}

        /// <inheritdoc />
        public virtual void EnterExceptionHandlerStatement(ExceptionHandlerStatement<TInstruction> statement) {}

        /// <inheritdoc />
        public virtual void ExitExceptionHandlerBlock(ExceptionHandlerStatement<TInstruction> statement) {}

        /// <inheritdoc />
        public virtual void EnterProtectedBlock(ExceptionHandlerStatement<TInstruction> statement) {}

        /// <inheritdoc />
        public virtual void ExitProtectedBlock(ExceptionHandlerStatement<TInstruction> statement) {}
        
        /// <inheritdoc />
        public virtual void EnterHandlerBlock(ExceptionHandlerStatement<TInstruction> statement, int handlerIndex) {}

        /// <inheritdoc />
        public virtual void ExitHandlerBlock(ExceptionHandlerStatement<TInstruction> statement, int handlerIndex) {}

        /// <inheritdoc />
        public virtual void EnterPrologueBlock(HandlerClause<TInstruction> clause) {}

        /// <inheritdoc />
        public virtual void ExitPrologueBlock(HandlerClause<TInstruction> clause) {}

        /// <inheritdoc />
        public virtual void EnterEpilogueBlock(HandlerClause<TInstruction> clause) {}

        /// <inheritdoc />
        public virtual void ExitEpilogueBlock(HandlerClause<TInstruction> clause) {}

        /// <inheritdoc />
        public virtual void EnterHandlerContents(HandlerClause<TInstruction> clause) {}

        /// <inheritdoc />
        public virtual void ExitHandlerContents(HandlerClause<TInstruction> clause) {}

        /// <inheritdoc />
        public virtual void EnterVariableExpression(VariableExpression<TInstruction> expression) {}
        
        /// <inheritdoc />
        public virtual void ExitVariableExpression(VariableExpression<TInstruction> expression) {}
        
        /// <inheritdoc />
        public virtual void EnterInstructionExpression(InstructionExpression<TInstruction> expression) {}
        
        /// <inheritdoc />
        public virtual void ExitInstructionExpression(InstructionExpression<TInstruction> expression) {}
    }
}