using System.Collections.Generic;

namespace Echo.Ast
{
    /// <summary>
    /// Represents and expression statement in the AST
    /// </summary>
    public sealed class AstExpressionStatement<TInstruction> : AstStatement<TInstruction>
    {
        /// <summary>
        /// Creates a new expression statement
        /// </summary>
        /// <param name="id">The unique ID to assign to the node</param>
        /// <param name="expression">The expression</param>
        public AstExpressionStatement(long id, AstExpressionBase<TInstruction> expression)
            : base(id)
        {
            Expression = expression;
        }

        /// <summary>
        /// The expression that this <see cref="AstExpressionStatement{TInstruction}"/> holds
        /// </summary>
        public AstExpressionBase<TInstruction> Expression
        {
            get;
        }

        /// <inheritdoc />
        public override IEnumerable<AstNodeBase<TInstruction>> GetChildren()
        {
            yield return Expression;
        }

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);
    }
}