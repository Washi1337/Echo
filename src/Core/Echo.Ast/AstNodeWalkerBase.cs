namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for Ast walkers
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction</typeparam>
    public abstract class AstNodeWalkerBase<TInstruction> : IAstNodeVisitor<TInstruction, object>
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

        private void VisitChildren(AstNodeBase<TInstruction> node)
        {
            foreach (var child in node.GetChildren())
                ((AstNodeBase<TInstruction>) child).Accept(this, null);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object>.Visit(AssignmentStatement<TInstruction> assignmentStatement,
            object state)
        {
            EnterAssignmentStatement(assignmentStatement);

            assignmentStatement.Expression.Accept(this, state);

            ExitAssignmentStatement(assignmentStatement);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object>.Visit(ExpressionStatement<TInstruction> expressionStatement,
            object state)
        {
            EnterExpressionStatement(expressionStatement);

            expressionStatement.Expression.Accept(this, state);

            ExitExpressionStatement(expressionStatement);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object>.Visit(PhiStatement<TInstruction> phiStatement, object state)
        {
            EnterPhiStatement(phiStatement);

            foreach (var source in phiStatement.Sources)
                source.Accept(this, state);

            ExitPhiStatement(phiStatement);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object>.Visit(InstructionExpression<TInstruction> instructionExpression, object state)
        {
            EnterInstructionExpression(instructionExpression);

            foreach (var parameter in instructionExpression.Arguments)
                parameter.Accept(this, state);

            ExitInstructionExpression(instructionExpression);
        }

        /// <inheritdoc />
        void IAstNodeVisitor<TInstruction, object>.Visit(VariableExpression<TInstruction> variableExpression, object state)
        {
            VisitVariableExpression(variableExpression);
        }
    }
}