namespace Echo.Ast
{
    /// <summary>
    /// Provides a base implementation for an <see cref="IAstNodeListener{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
    public abstract class AstNodeListener<TInstruction> : IAstNodeListener<TInstruction>
    {
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
        public virtual void EnterVariableExpression(VariableExpression<TInstruction> expression) {}
        
        /// <inheritdoc />
        public virtual void ExitVariableExpression(VariableExpression<TInstruction> expression) {}
        
        /// <inheritdoc />
        public virtual void EnterInstructionExpression(InstructionExpression<TInstruction> expression) {}
        
        /// <inheritdoc />
        public virtual void ExitInstructionExpression(InstructionExpression<TInstruction> expression) {}
    }
}