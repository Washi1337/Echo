using System.Linq;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an assignment in the AST
    /// </summary>
    public sealed class AssignmentStatement<TInstruction> : StatementBase<TInstruction>
    {
        /// <summary>
        /// Creates a new assignment statement
        /// </summary>
        /// <param name="id">The unique ID to assign to the node</param>
        /// <param name="expression">The expression</param>
        /// <param name="variables">The variables</param>
        public AssignmentStatement(long id, ExpressionBase<TInstruction> expression, IVariable[] variables)
            : base(id)
        {
            Expression = expression;
            Variables = variables;
            Children.Add(expression);
        }

        /// <summary>
        /// The expression to assign to <see cref="Variables"/>
        /// </summary>
        public ExpressionBase<TInstruction> Expression
        {
            get;
        }

        /// <summary>
        /// The variables that will get assigned to
        /// </summary>
        public IVariable[] Variables
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
        public override string ToString() => $"{string.Join(", ", Variables.Select(v => v.ToString()))} = {Expression}";
        
        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) =>
           $"{string.Join(", ", Variables.Select(v => v.ToString()))} = {Expression.Format(instructionFormatter)}"; 
    }
}