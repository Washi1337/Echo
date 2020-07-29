﻿using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Represents an assignment in the AST
    /// </summary>
    public sealed class AstAssignmentStatement<TInstruction> : AstStatementBase<TInstruction>
    {
        /// <summary>
        /// Creates a new assignment statement
        /// </summary>
        /// <param name="id">The unique ID to assign to the node</param>
        /// <param name="expression">The expression</param>
        /// <param name="variables">The variables</param>
        public AstAssignmentStatement(long id, AstExpressionBase<TInstruction> expression, IVariable[] variables)
            : base(id)
        {
            Expression = expression;
            Variables = variables;
        }

        /// <summary>
        /// The expression to assign to <see cref="Variables"/>
        /// </summary>
        public AstExpressionBase<TInstruction> Expression
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
        public override IEnumerable<AstNodeBase<TInstruction>> GetChildren()
        {
            yield return Expression;

            foreach (var variable in Variables)
                yield return new AstVariableExpression<TInstruction>(0, variable);
        }

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);
    }
}