using System.Collections.Generic;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Code;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a Phi statement in the Ast
    /// </summary>
    public sealed class PhiStatement<TInstruction> : Statement<TInstruction>
    {
        /// <summary>
        /// Creates a new Phi statement
        /// </summary>
        /// <param name="target">The target variable that will be assigned to</param>
        /// <param name="sources">The possible sources for the assignment</param>
        public PhiStatement(IVariable target, IEnumerable<VariableExpression<TInstruction>> sources)
        {
            Sources = new TreeNodeCollection<PhiStatement<TInstruction>, VariableExpression<TInstruction>>(this);
            Target = target;
            
            foreach (var source in sources)
                Sources.Add(source);
        }

        /// <summary>
        /// The variable that will be assigned to
        /// </summary>
        public IVariable Target
        {
            get;
            private set;
        }

        /// <summary>
        /// The possible sources for that could be assigned to <see cref="Target"/>
        /// </summary>
        public ICollection<VariableExpression<TInstruction>> Sources
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren() => Sources;

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        public PhiStatement<TInstruction> WithTarget(IVariable target)
        {
            Target = target;
            return this;
        }

        public PhiStatement<TInstruction> WithSources(params VariableExpression<TInstruction>[] sources) =>
            WithSources(sources as IEnumerable<VariableExpression<TInstruction>>);

        public PhiStatement<TInstruction> WithSources(IEnumerable<VariableExpression<TInstruction>> sources)
        {
            var collection = new TreeNodeCollection<PhiStatement<TInstruction>, VariableExpression<TInstruction>>(this);
            foreach (var source in sources)
                collection.Add(source);

            Sources = collection;
            return this;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Target} = φ({string.Join(", ", Sources)})";

        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) => ToString();
    }
}