namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for Ast walkers
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction</typeparam>
    /// <typeparam name="TState">The type of the state to pass</typeparam>
    public abstract class NodeWalkerBase<TInstruction, TState> : INodeVisitor<TInstruction, TState>
    {
        /// <summary>
        /// Begin visiting a given <see cref="AssignmentStatement{TInstruction}"/>
        /// </summary>
        /// <param name="assignmentStatement">The <see cref="AssignmentStatement{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void EnterAssignmentStatement(
            AssignmentStatement<TInstruction> assignmentStatement, TState state) { }

        /// <summary>
        /// Finish visiting a given <see cref="AssignmentStatement{TInstruction}"/>
        /// </summary>
        /// <param name="assignmentStatement">The <see cref="AssignmentStatement{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void ExitAssignmentStatement(
            AssignmentStatement<TInstruction> assignmentStatement, TState state) { }

        /// <summary>
        /// Begin visiting a given <see cref="ExpressionStatement{TInstruction}"/>
        /// </summary>
        /// <param name="expressionStatement">The <see cref="ExpressionStatement{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void EnterExpressionStatement(
            ExpressionStatement<TInstruction> expressionStatement, TState state) { }
        
        /// <summary>
        /// Finish visiting a given <see cref="ExpressionStatement{TInstruction}"/>
        /// </summary>
        /// <param name="expressionStatement">The <see cref="ExpressionStatement{TInstruction}"/> that is being finished</param>
        /// <param name="state">The state</param>
        protected virtual void ExitExpressionStatement(
            ExpressionStatement<TInstruction> expressionStatement, TState state) { }

        /// <summary>
        /// Begin visiting a given <see cref="PhiStatement{TInstruction}"/>
        /// </summary>
        /// <param name="phiStatement">The <see cref="PhiStatement{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void EnterPhiStatement(
            PhiStatement<TInstruction> phiStatement, TState state) { }

        /// <summary>
        /// Finish visiting a given <see cref="PhiStatement{TInstruction}"/>
        /// </summary>
        /// <param name="phiStatement">The <see cref="PhiStatement{TInstruction}"/> that is being finished</param>
        /// <param name="state">The state</param>
        protected virtual void ExitPhiStatement(
            PhiStatement<TInstruction> phiStatement, TState state) { }

        /// <summary>
        /// Begin visiting a given <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        /// <param name="instructionExpression">The <see cref="InstructionExpression{TInstruction}"/> that is being entered</param>
        /// <param name="state">The state</param>
        protected virtual void EnterInstructionExpression(
            InstructionExpression<TInstruction> instructionExpression, TState state) { }

        /// <summary>
        /// Finish visiting a given <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        /// <param name="instructionExpression">The <see cref="InstructionExpression{TInstruction}"/> that is being finished</param>
        /// <param name="state">The state</param>
        protected virtual void ExitInstructionExpression(
            InstructionExpression<TInstruction> instructionExpression, TState state) { }

        /// <summary>
        /// Visiting a given <see cref="VariableExpression{TInstruction}"/>
        /// </summary>
        /// <param name="variableExpression">The <see cref="VariableExpression{TInstruction}"/> that is will be visited</param>
        /// <param name="state">The state</param>
        protected virtual void VisitVariableExpression(
            VariableExpression<TInstruction> variableExpression, TState state) { }

        /// <inheritdoc />
        void INodeVisitor<TInstruction, TState>.Visit(AssignmentStatement<TInstruction> assignmentStatement, TState state)
        {
            EnterAssignmentStatement(assignmentStatement, state);

            assignmentStatement.Expression.Accept(this, state);
            
            ExitAssignmentStatement(assignmentStatement, state);
        }

        /// <inheritdoc />
        void INodeVisitor<TInstruction, TState>.Visit(ExpressionStatement<TInstruction> expressionStatement, TState state)
        {
            EnterExpressionStatement(expressionStatement, state);

            expressionStatement.Expression.Accept(this, state);

            ExitExpressionStatement(expressionStatement, state);
        }

        /// <inheritdoc />
        void INodeVisitor<TInstruction, TState>.Visit(PhiStatement<TInstruction> phiStatement, TState state)
        {
            EnterPhiStatement(phiStatement, state);
            
            foreach (var source in phiStatement.Sources)
                source.Accept(this, state);
            
            ExitPhiStatement(phiStatement, state);
        }

        /// <inheritdoc />
        void INodeVisitor<TInstruction, TState>.Visit(InstructionExpression<TInstruction> instructionExpression, TState state)
        {
            EnterInstructionExpression(instructionExpression, state);

            foreach (var parameter in instructionExpression.GetChildren())
                ((ExpressionBase<TInstruction>) parameter).Accept(this, state);
            
            ExitInstructionExpression(instructionExpression, state);
        }

        /// <inheritdoc />
        void INodeVisitor<TInstruction, TState>.Visit(VariableExpression<TInstruction> variableExpression, TState state)
        {
            VisitVariableExpression(variableExpression, state);
        }
    }
}