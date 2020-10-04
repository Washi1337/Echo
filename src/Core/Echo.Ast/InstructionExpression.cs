using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an instruction in the AST
    /// </summary>
    public sealed class InstructionExpression<TInstruction> : Expression<TInstruction>
    {
        /// <summary>
        /// Creates a new instruction expression node
        /// </summary>
        /// <param name="content">The instruction</param>
        /// <param name="arguments">The parameters to this instruction</param>
        public InstructionExpression(TInstruction content, IEnumerable<Expression<TInstruction>> arguments)
        {
            Content = content;
            Arguments = arguments.ToList();
        }

        /// <summary>
        /// The instruction that the AST node represents
        /// </summary>
        public TInstruction Content
        {
            get;
        }

        /// <summary>
        /// The arguments to this <see cref="InstructionExpression{TInstruction}"/>
        /// </summary>
        public IList<Expression<TInstruction>> Arguments
        {
            get;
        }

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren() => Arguments;

        /// <inheritdoc />
        public override string ToString() => $"{Content}({string.Join(", ", GetChildren())})";

        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) =>
            $"{instructionFormatter.Format(Content)}({string.Join(", ", GetChildren())})";
    }
}
