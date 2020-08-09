using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a variable expression in the AST
    /// </summary>
    public sealed class VariableExpression<TInstruction> : ExpressionBase<TInstruction>
    {
        /// <summary>
        /// Creates a new variable expression
        /// </summary>
        /// <param name="id">The unique ID to give to the node</param>
        /// <param name="variable">The variable</param>
        public VariableExpression(long id, IVariable variable)
            : base(id)
        {
            Variable = variable;
        }

        /// <summary>
        /// The variable that is represented by the AST node
        /// </summary>
        public IVariable Variable
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
        public override string ToString() => $"{Variable}";

        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) => ToString();
    }
}