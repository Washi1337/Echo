namespace Echo.Ast
{
    /// <summary>
    /// Provides a visitor interface to implement a visitor pattern on the AST
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction the AST models</typeparam>
    public interface IAstNodeVisitor<TInstruction>
    {
        /// <summary>
        /// Visits a given <see cref="AssignmentStatement{TInstruction}"/>
        /// </summary>
        void Visit(AssignmentStatement<TInstruction> statement);

        /// <summary>
        /// Visits a given <see cref="ExpressionStatement{TInstruction}"/>
        /// </summary>
        void Visit(ExpressionStatement<TInstruction> expression);

        /// <summary>
        /// Visits a given <see cref="PhiStatement{TInstruction}"/>
        /// </summary>
        void Visit(PhiStatement<TInstruction> statement);
        
        /// <summary>
        /// Visits a given <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        void Visit(InstructionExpression<TInstruction> expression);

        /// <summary>
        /// Visits a given <see cref="VariableExpression{TInstruction}"/>
        /// </summary>
        void Visit(VariableExpression<TInstruction> expression);
    }
    
    /// <summary>
    /// Provides a visitor interface to implement a visitor pattern on the AST
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction the AST models</typeparam>
    /// <typeparam name="TState">The state to pass between visitors</typeparam>
    public interface IAstNodeVisitor<TInstruction, in TState>
    {
        /// <summary>
        /// Visits a given <see cref="AssignmentStatement{TInstruction}"/>
        /// </summary>
        void Visit(AssignmentStatement<TInstruction> statement, TState state);

        /// <summary>
        /// Visits a given <see cref="ExpressionStatement{TInstruction}"/>
        /// </summary>
        void Visit(ExpressionStatement<TInstruction> expression, TState state);

        /// <summary>
        /// Visits a given <see cref="PhiStatement{TInstruction}"/>
        /// </summary>
        void Visit(PhiStatement<TInstruction> statement, TState state);
        
        /// <summary>
        /// Visits a given <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        void Visit(InstructionExpression<TInstruction> expression, TState state);

        /// <summary>
        /// Visits a given <see cref="VariableExpression{TInstruction}"/>
        /// </summary>
        void Visit(VariableExpression<TInstruction> expression, TState state);
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
        /// Visits a given <see cref="AssignmentStatement{TInstruction}"/>
        /// </summary>
        TOut Visit(AssignmentStatement<TInstruction> statement, TState state);

        /// <summary>
        /// Visits a given <see cref="ExpressionStatement{TInstruction}"/>
        /// </summary>
        TOut Visit(ExpressionStatement<TInstruction> statement, TState state);

        /// <summary>
        /// Visits a given <see cref="PhiStatement{TInstruction}"/>
        /// </summary>
        TOut Visit(PhiStatement<TInstruction> statement, TState state);
        
        /// <summary>
        /// Visits a given <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        TOut Visit(InstructionExpression<TInstruction> expression, TState state);

        /// <summary>
        /// Visits a given <see cref="VariableExpression{TInstruction}"/>
        /// </summary>
        TOut Visit(VariableExpression<TInstruction> expression, TState state);
    }
}