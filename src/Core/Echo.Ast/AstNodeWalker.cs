namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for Ast walkers
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction</typeparam>
    public abstract class AstNodeWalker<TInstruction> : IAstNodeVisitor<TInstruction, object?>
    {
        /// <summary>
        /// Begin visiting a given <see cref="AssignmentStatement{TInstruction}"/>
        /// </summary>
        /// <param name="assignmentStatement">The <see cref="AssignmentStatement{TInstruction}"/> that is being entered</param>
        protected virtual void EnterAssignmentStatement(AssignmentStatement<TInstruction> assignmentStatement) =>
            VisitChildren(assignmentStatement);

        /// <summary>
        /// Finish visiting a given <see cref="AssignmentStatement{TInstruction}"/>
        /// </summary>
        /// <param name="assignmentStatement">The <see cref="AssignmentStatement{TInstruction}"/> that is being entered</param>
        protected virtual void ExitAssignmentStatement(AssignmentStatement<TInstruction> assignmentStatement) { }

        /// <summary>
        /// Begin visiting a given <see cref="ExpressionStatement{TInstruction}"/>
        /// </summary>
        /// <param name="expressionStatement">The <see cref="ExpressionStatement{TInstruction}"/> that is being entered</param>
        protected virtual void EnterExpressionStatement(ExpressionStatement<TInstruction> expressionStatement) =>
            VisitChildren(expressionStatement);

        /// <summary>
        /// Finish visiting a given <see cref="ExpressionStatement{TInstruction}"/>
        /// </summary>
        /// <param name="expressionStatement">The <see cref="ExpressionStatement{TInstruction}"/> that is being finished</param>
        protected virtual void ExitExpressionStatement(ExpressionStatement<TInstruction> expressionStatement) { }

        /// <summary>
        /// Begin visiting a given <see cref="PhiStatement{TInstruction}"/>
        /// </summary>
        /// <param name="phiStatement">The <see cref="PhiStatement{TInstruction}"/> that is being entered</param>
        protected virtual void EnterPhiStatement(PhiStatement<TInstruction> phiStatement) =>
            VisitChildren(phiStatement);

        /// <summary>
        /// Finish visiting a given <see cref="PhiStatement{TInstruction}"/>
        /// </summary>
        /// <param name="phiStatement">The <see cref="PhiStatement{TInstruction}"/> that is being finished</param>
        protected virtual void ExitPhiStatement(PhiStatement<TInstruction> phiStatement) { }

        /// <summary>
        /// Begin visiting a given <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        /// <param name="instructionExpression">The <see cref="InstructionExpression{TInstruction}"/> that is being entered</param>
        protected virtual void EnterInstructionExpression(InstructionExpression<TInstruction> instructionExpression) =>
            VisitChildren(instructionExpression);

        /// <summary>
        /// Finish visiting a given <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        /// <param name="instructionExpression">The <see cref="InstructionExpression{TInstruction}"/> that is being finished</param>
        protected virtual void ExitInstructionExpression(InstructionExpression<TInstruction> instructionExpression) { }

        /// <summary>
        /// Visiting a given <see cref="VariableExpression{TInstruction}"/>
        /// </summary>
        /// <param name="variableExpression">The <see cref="VariableExpression{TInstruction}"/> that is will be visited</param>
        protected virtual void VisitVariableExpression(VariableExpression<TInstruction> variableExpression) { }

        private void VisitChildren(AstNode<TInstruction> node)
        {
            foreach (var child in node.GetChildren())
                ((AstNode<TInstruction>) child).Accept(this, null);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object?>.Visit(AssignmentStatement<TInstruction> statement, object? state)
        {
            EnterAssignmentStatement(statement);

            statement.Expression.Accept(this, state);

            ExitAssignmentStatement(statement);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object?>.Visit(ExpressionStatement<TInstruction> expression, object? state)
        {
            EnterExpressionStatement(expression);

            expression.Expression.Accept(this, state);

            ExitExpressionStatement(expression);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object?>.Visit(PhiStatement<TInstruction> statement, object? state)
        {
            EnterPhiStatement(statement);

            foreach (var source in statement.Sources)
                source.Accept(this, state);

            ExitPhiStatement(statement);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object?>.Visit(InstructionExpression<TInstruction> expression, object? state)
        {
            EnterInstructionExpression(expression);

            foreach (var parameter in expression.Arguments)
                parameter.Accept(this, state);

            ExitInstructionExpression(expression);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object?>.Visit(VariableExpression<TInstruction> expression, object? state)
        {
            VisitVariableExpression(expression);
        }
    }
}