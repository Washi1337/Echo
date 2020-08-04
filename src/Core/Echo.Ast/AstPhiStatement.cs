using System.Collections.Generic;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a Phi statement in the Ast
    /// </summary>
    public sealed class AstPhiStatement<TInstruction> : AstStatementBase<TInstruction>
    {
        /// <summary>
        /// Creates a new Phi statement
        /// </summary>
        /// <param name="id">The unique ID to assign to the node</param>
        /// <param name="sources">The possible sources for the assignment</param>
        /// <param name="target">The target variable that will be assigned to</param>
        public AstPhiStatement(long id, ICollection<AstVariableExpression<TInstruction>> sources, IVariable target)
            : base(id)
        {
            Sources = sources;
            Target = target;
            
            foreach (var source in sources)
                Children.Add(source);
        }

        /// <summary>
        /// The possible sources for that could be assigned to <see cref="Target"/>
        /// </summary>
        public ICollection<AstVariableExpression<TInstruction>> Sources
        {
            get;
        }

        /// <summary>
        /// The variable that will be assigned to
        /// </summary>
        public IVariable Target
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
        public override string ToString() => $"{Target} = φ({string.Join(", ", Sources)})";

        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) => ToString();
    }
}