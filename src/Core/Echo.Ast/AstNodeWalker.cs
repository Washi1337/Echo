namespace Echo.Ast
{
    /// <summary>
    /// Provides a mechanism for performing a full traversal of an abstract syntax tree (AST).
    /// </summary>
    /// <typeparam name="TInstruction">The instruction stored in the AST.</typeparam>
    public class AstNodeWalker<TInstruction> : IAstNodeVisitor<TInstruction>
        where TInstruction : notnull
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
        public void Visit(CompilationUnit<TInstruction> unit)
        {
            _listener.EnterCompilationUnit(unit);
            unit.Root.Accept(this);
            _listener.ExitCompilationUnit(unit);
        }

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
        public void Visit(BlockStatement<TInstruction> statement)
        {
            _listener.EnterBlockStatement(statement);
            foreach (var s in statement.Statements)
                s.Accept(this);
            _listener.ExitBlockStatement(statement);
        }

        /// <inheritdoc />
        public void Visit(ExceptionHandlerStatement<TInstruction> statement)
        {
            _listener.EnterExceptionHandlerStatement(statement);

            _listener.EnterProtectedBlock(statement);
            statement.ProtectedBlock.Accept(this);
            _listener.ExitProtectedBlock(statement);

            for (int i = 0; i < statement.Handlers.Count; i++)
            {
                var handlerBlock = statement.Handlers[i];
                
                _listener.EnterHandlerBlock(statement, i);
                handlerBlock.Accept(this);
                _listener.ExitHandlerBlock(statement, i);
            }
            
            _listener.ExitExceptionHandlerBlock(statement);
        }

        /// <inheritdoc />
        public void Visit(HandlerClause<TInstruction> clause)
        {
            if (clause.Prologue is not null)
            {
                _listener.EnterPrologueBlock(clause);
                clause.Prologue.Accept(this);
                _listener.ExitPrologueBlock(clause);
            }

            _listener.EnterHandlerContents(clause);
            clause.Contents.Accept(this);
            _listener.ExitHandlerContents(clause);
            
            if (clause.Epilogue is not null)
            {
                _listener.EnterEpilogueBlock(clause);
                clause.Epilogue.Accept(this);
                _listener.ExitEpilogueBlock(clause);
            }
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