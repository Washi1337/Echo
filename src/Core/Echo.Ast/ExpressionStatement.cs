using System.Collections.Generic;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents and expression statement in the AST
    /// </summary>
    public sealed class ExpressionStatement<TInstruction> : Statement<TInstruction>
    {
        private Expression<TInstruction> _expression;

        /// <summary>
        /// Creates a new expression statement
        /// </summary>
        /// <param name="expression">The expression</param>
        public ExpressionStatement(Expression<TInstruction> expression) => Expression = expression;

        /// <summary>
        /// The expression that this <see cref="ExpressionStatement{TInstruction}"/> holds
        /// </summary>
        public Expression<TInstruction> Expression
        {
            get => _expression;
            set => UpdateChild(ref _expression, value);
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren()
        {
            yield return _expression;
        }

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        public ExpressionStatement<TInstruction> WithExpression(Expression<TInstruction> expression)
        {
            Expression = expression;
            return this;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Expression}";

        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) =>
            $"{Expression.Format(instructionFormatter)}";
    }
}