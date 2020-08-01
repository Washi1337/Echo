using System.Collections.Generic;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an instruction in the AST
    /// </summary>
    public sealed class AstInstructionExpression<TInstruction> : AstExpressionBase<TInstruction>
    {
        /// <summary>
        /// Creates a new instruction expression node
        /// </summary>
        /// <param name="id">The unique ID to give to the node</param>
        /// <param name="content">The instruction</param>
        /// <param name="parameters">The parameters to this instruction</param>
        public AstInstructionExpression(long id, TInstruction content, ICollection<AstExpressionBase<TInstruction>> parameters)
            : base(id)
        {
            Content = content;
            Parameters = parameters;
            
            foreach (var parameter in parameters)
                Children.Add(parameter);
        }

        /// <summary>
        /// The instruction that the AST node represents
        /// </summary>
        public TInstruction Content
        {
            get;
        }

        /// <summary>
        /// Gets the parameters for the expression
        /// </summary>
        public ICollection<AstExpressionBase<TInstruction>> Parameters
        {
            get;
        }

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);
    }
}