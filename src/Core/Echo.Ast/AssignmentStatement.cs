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
        /// <summary>
        /// Creates a new assignment statement
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <param name="variables">The variables</param>
        public AssignmentStatement(Expression<TInstruction> expression, IVariable[] variables)
        {
            Expression = expression;
            Variables = variables;
        }

        /// <summary>
        /// The expression to assign to <see cref="Variables"/>
        /// </summary>
        public Expression<TInstruction> Expression
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
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override string ToString() => $"{string.Join(", ", Variables.Select(v => v.Name))} = {Expression}";
        
        internal override string Format(IInstructionFormatter<TInstruction> instructionFormatter) =>
           $"{string.Join(", ", Variables.Select(v => v.Name))} = {Expression.Format(instructionFormatter)}"; 
    }
}
