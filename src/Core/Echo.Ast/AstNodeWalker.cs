namespace Echo.Ast
{
    /// <summary>
    /// Provides a mechanism for performing a full traversal of an abstract syntax tree (AST).
    /// </summary>
    /// <typeparam name="TInstruction">The instruction stored in the AST.</typeparam>
    public class AstNodeWalker<TInstruction> : IAstNodeVisitor<TInstruction>
    {
        private readonly IAstNodeListener<TInstruction> _listener;

        /// <summary>
        /// Creates a new AST node walker.
        /// </summary>
        /// <param name="listener">The listener to call after every traversal step.</param>
        public AstNodeWalker(IAstNodeListener<TInstruction> listener)
        {
            _listener = listener;
        }

        /// <summary>
        /// Walks the provided AST with the provided listener.
        /// </summary>
        /// <param name="listener">The listener to callback after every traversal step.</param>
        /// <param name="node">The root node of the AST.</param>
        public static void Walk(IAstNodeListener<TInstruction> listener, AstNode<TInstruction> node)
        {
            var walker = new AstNodeWalker<TInstruction>(listener);
            walker.Walk(node);
        }

        /// <summary>
        /// Walks the provided AST.
        /// </summary>
        /// <param name="node">The root node of the AST.</param>
        public void Walk(AstNode<TInstruction> node) => node.Accept(this);

        /// <inheritdoc />
        public void Visit(AssignmentStatement<TInstruction> statement)
        {
            _listener.EnterAssignmentStatement(statement);
            statement.Expression.Accept(this);
            _listener.ExitAssignmentStatement(statement);
        }

        /// <inheritdoc />
        public void Visit(ExpressionStatement<TInstruction> expression)
        {
            _listener.EnterExpressionStatement(expression);
            expression.Expression.Accept(this);
            _listener.ExitExpressionStatement(expression);
        }

        /// <inheritdoc />
        public void Visit(PhiStatement<TInstruction> statement)
        {
            _listener.EnterPhiStatement(statement);
            for (int i = 0; i < statement.Sources.Count; i++)
                statement.Sources[i].Accept(this);
            _listener.ExitPhiStatement(statement);
        }

        /// <inheritdoc />
        public void Visit(InstructionExpression<TInstruction> expression)
        {
            _listener.EnterInstructionExpression(expression);
            for (int i = 0; i < expression.Arguments.Count; i++)
                expression.Arguments[i].Accept(this);
            _listener.ExitInstructionExpression(expression);
        }

        /// <inheritdoc />
        public void Visit(VariableExpression<TInstruction> expression)
        {
            _listener.EnterVariableExpression(expression);
            _listener.ExitVariableExpression(expression);
        }
    }
}