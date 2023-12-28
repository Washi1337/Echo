using System.Collections.Generic;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Code;
using Echo.Graphing;

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
        /// <param name="representative">The target variable that will be assigned to</param>
        public PhiStatement(IVariable representative)
        {
            Representative = representative;
            Sources = new TreeNodeCollection<PhiStatement<TInstruction>, VariableExpression<TInstruction>>(this);
        }
        
        /// <summary>
        /// Creates a new Phi statement
        /// </summary>
        /// <param name="representative">The target variable that will be assigned to</param>
        /// <param name="sources">The possible sources for the assignment</param>
        public PhiStatement(IVariable representative, IEnumerable<VariableExpression<TInstruction>> sources)
        {
            Representative = representative;
            
            Sources = new TreeNodeCollection<PhiStatement<TInstruction>, VariableExpression<TInstruction>>(this);
            foreach (var source in sources)
                Sources.Add(source);
        }

        /// <summary>
        /// The variable that will be assigned to
        /// </summary>
        public IVariable Representative
        {
            get;
            private set;
        }

        /// <summary>
        /// The possible sources for that could be assigned to <see cref="Representative"/>
        /// </summary>
        public IList<VariableExpression<TInstruction>> Sources
        {
            get;
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren() => Sources;

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <summary>
        /// Modifies the current <see cref="PhiStatement{TInstruction}"/> to assign to <paramref name="variable"/>
        /// </summary>
        /// <param name="variable">The new target to assign</param>
        /// <returns>The same <see cref="PhiStatement{TInstruction}"/> instance but with the new <paramref name="variable"/></returns>
        public PhiStatement<TInstruction> WithRepresentative(IVariable variable)
        {
            Representative = variable;
            return this;
        }

        /// <summary>
        /// Modifies the current <see cref="PhiStatement{TInstruction}"/> to assign values from <paramref name="sources"/>
        /// </summary>
        /// <param name="sources">The sources to get values from</param>
        /// <returns>The same <see cref="PhiStatement{TInstruction}"/> instance but with the new <paramref name="sources"/></returns>
        public PhiStatement<TInstruction> WithSources(params VariableExpression<TInstruction>[] sources) =>
            WithSources(sources as IEnumerable<VariableExpression<TInstruction>>);

        /// <summary>
        /// Modifies the current <see cref="PhiStatement{TInstruction}"/> to assign values from <paramref name="sources"/>
        /// </summary>
        /// <param name="sources">The sources to get values from</param>
        /// <returns>The same <see cref="PhiStatement{TInstruction}"/> instance but with the new <paramref name="sources"/></returns>
        public PhiStatement<TInstruction> WithSources(IEnumerable<VariableExpression<TInstruction>> sources)
        {
            Sources.Clear();
            
            foreach (var source in sources)
                Sources.Add(source);

            return this;
        }
    }
}