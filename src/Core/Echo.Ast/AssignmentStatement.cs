using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Code;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an assignment in the AST
    /// </summary>
    public sealed class AssignmentStatement<TInstruction> : Statement<TInstruction>
    {
        private Expression<TInstruction> _expression;

        /// <summary>
        /// Creates a new assignment statement
        /// </summary>
        /// <param name="variable">The variable</param>
        /// <param name="expression">The expression</param>
        public AssignmentStatement(IVariable variable, Expression<TInstruction> expression)
            : this(new[] { variable }, expression) { }

        /// <summary>
        /// Creates a new assignment statement
        /// </summary>
        /// <param name="variables">The variables</param>
        /// <param name="expression">The expression</param>
        public AssignmentStatement(IEnumerable<IVariable> variables, Expression<TInstruction> expression)
        {
            Expression = expression;
            Variables = variables.ToList();
        }

        /// <summary>
        /// The variables that will get assigned to
        /// </summary>
        public IList<IVariable> Variables
        {
            get;
            private set;
        }

        /// <summary>
        /// The expression to assign to <see cref="Variables"/>
        /// </summary>
        public Expression<TInstruction> Expression
        {
            get => _expression;
            set => UpdateChild(ref _expression, value);
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren()
        {
            yield return Expression;
        }

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        public AssignmentStatement<TInstruction> WithVariables(params IVariable[] variables) =>
            WithVariables(variables as IEnumerable<IVariable>);

        public AssignmentStatement<TInstruction> WithVariables(IEnumerable<IVariable> variables)
        {
            Variables = new List<IVariable>(variables);
            return this;
        }

        public AssignmentStatement<TInstruction> WithExpression(Expression<TInstruction> expression)
        {
            Expression = expression;
            return this;
        }

        /// <inheritdoc />
        public override string ToString() => $"{string.Join(", ", Variables.Select(v => v.Name))} = {Expression}";
        
        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) =>
           $"{string.Join(", ", Variables.Select(v => v.Name))} = {Expression.Format(instructionFormatter)}"; 
    }
}
