using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Ast
{
    /// <summary>
    /// Represents and expression statement in the AST
    /// </summary>
    public sealed class ExpressionStatement<TInstruction> : StatementBase<TInstruction>
    {
        /// <summary>
        /// Creates a new expression statement
        /// </summary>
        /// <param name="id">The unique ID to assign to the node</param>
        /// <param name="expression">The expression</param>
        public ExpressionStatement(long id, ExpressionBase<TInstruction> expression)
            : base(id)
        {
            Expression = expression;
            Children.Add(expression);
        }

        /// <summary>
        /// The expression that this <see cref="ExpressionStatement{TInstruction}"/> holds
        /// </summary>
        public ExpressionBase<TInstruction> Expression
        {
            get;
        }

        /// <inheritdoc />
        public override void Accept<TState>(INodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(INodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override string ToString() => $"{Expression}";

        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) =>
            $"{Expression.Format(instructionFormatter)}";
    }
}