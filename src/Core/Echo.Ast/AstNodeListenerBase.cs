namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for listeners
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction</typeparam>
    /// <typeparam name="TState">The type of the state to pass</typeparam>
    public abstract class AstNodeListenerBase<TInstruction, TState> : IAstNodeVisitor<TInstruction, TState>
    {
        /// <summary>
        /// Begin visiting a given <see cref="AstAssignmentStatement{TInstruction}"/>
        /// </summary>
        /// <param name="assignmentStatement">The <see cref="AstAssignmentStatement{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void EnterAssignmentStatement(
            AstAssignmentStatement<TInstruction> assignmentStatement, TState state) { }

        /// <summary>
        /// Finish visiting a given <see cref="AstAssignmentStatement{TInstruction}"/>
        /// </summary>
        /// <param name="assignmentStatement">The <see cref="AstAssignmentStatement{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void ExitAssignmentStatement(
            AstAssignmentStatement<TInstruction> assignmentStatement, TState state) { }

        /// <summary>
        /// Begin visiting a given <see cref="AstExpressionStatement{TInstruction}"/>
        /// </summary>
        /// <param name="expressionStatement">The <see cref="AstExpressionStatement{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void EnterExpressionStatement(
            AstExpressionStatement<TInstruction> expressionStatement, TState state) { }

        /// <summary>
        /// Finish visiting a given <see cref="AstExpressionStatement{TInstruction}"/>
        /// </summary>
        /// <param name="expressionStatement">The <see cref="AstExpressionStatement{TInstruction}"/> that is being finished</param>
        /// <param name="state">The state</param>
        protected virtual void ExitExpressionStatement(
            AstExpressionStatement<TInstruction> expressionStatement, TState state) { }

        /// <summary>
        /// Begin visiting a given <see cref="AstInstructionExpression{TInstruction}"/>
        /// </summary>
        /// <param name="instructionExpression">The <see cref="AstInstructionExpression{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void EnterInstructionExpression(
            AstInstructionExpression<TInstruction> instructionExpression, TState state) { }

        /// <summary>
        /// Finish visiting a given <see cref="AstInstructionExpression{TInstruction}"/>
        /// </summary>
        /// <param name="instructionExpression">The <see cref="AstInstructionExpression{TInstruction}"/> that is being finished</param>
        /// <param name="state">The state</param>
        protected virtual void ExitInstructionExpression(
            AstInstructionExpression<TInstruction> instructionExpression, TState state) { }

        /// <summary>
        /// Visiting a given <see cref="AstVariableExpression{TInstruction}"/>
        /// </summary>
        /// <param name="variableExpression">The <see cref="AstVariableExpression{TInstruction}"/> that is will be visited</param>
        /// <param name="state">The state</param>
        protected virtual void VisitVariableExpression(
            AstVariableExpression<TInstruction> variableExpression, TState state) { }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, TState>.Visit(AstAssignmentStatement<TInstruction> assignmentStatement, TState state)
        {
            EnterAssignmentStatement(assignmentStatement, state);

            assignmentStatement.Expression.Accept(this, state);
            
            ExitAssignmentStatement(assignmentStatement, state);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, TState>.Visit(AstExpressionStatement<TInstruction> expressionStatement, TState state)
        {
            EnterExpressionStatement(expressionStatement, state);

            expressionStatement.Expression.Accept(this, state);

            ExitExpressionStatement(expressionStatement, state);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, TState>.Visit(AstInstructionExpression<TInstruction> instructionExpression, TState state)
        {
            EnterInstructionExpression(instructionExpression, state);

            foreach (AstExpressionBase<TInstruction> parameter in instructionExpression.Parameters)
                parameter.Accept(this, state);
            
            ExitInstructionExpression(instructionExpression, state);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, TState>.Visit(AstVariableExpression<TInstruction> variableExpression, TState state)
        {
            VisitVariableExpression(variableExpression, state);
        }
    }
}