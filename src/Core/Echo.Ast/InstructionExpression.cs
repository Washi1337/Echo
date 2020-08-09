using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an instruction in the AST
    /// </summary>
    public sealed class InstructionExpression<TInstruction> : ExpressionBase<TInstruction>
    {
        /// <summary>
        /// Creates a new instruction expression node
        /// </summary>
        /// <param name="id">The unique ID to give to the node</param>
        /// <param name="content">The instruction</param>
        /// <param name="arguments">The parameters to this instruction</param>
        public InstructionExpression(long id, TInstruction content, IEnumerable<ExpressionBase<TInstruction>> arguments)
            : base(id)
        {
            Content = content;
            Arguments = new ArgumentCollection<TInstruction>(this, arguments);
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
        public ArgumentCollection<TInstruction> Arguments
        {
            get;
        }

        /// <inheritdoc />
        public override void Accept<TState>(INodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override string ToString() => $"{Content}({string.Join(", ", Arguments)})";

        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) =>
            $"{instructionFormatter.Format(Content)}({string.Join(", ", Arguments)})";
    }
}