﻿using System.Collections.Generic;
using Echo.Code;
using Echo.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a statement that assigns a value to a (set of) variable(s).
    /// </summary>
    public sealed class AssignmentStatement<TInstruction> : Statement<TInstruction>
        where TInstruction : notnull
    {
        private Expression<TInstruction> _expression = null!;

        /// <summary>
        /// Creates a new assignment statement
        /// </summary>
        /// <param name="variable">The variable</param>
        /// <param name="expression">The expression</param>
        public AssignmentStatement(IVariable variable, Expression<TInstruction> expression)
            : this(new[] {variable}, expression)
        {
        }

        /// <summary>
        /// Creates a new assignment statement
        /// </summary>
        /// <param name="variables">The variables</param>
        /// <param name="expression">The expression</param>
        public AssignmentStatement(IEnumerable<IVariable> variables, Expression<TInstruction> expression)
        {
            Expression = expression;
            Variables = new VariableCollection<TInstruction>(this);
            foreach (var variable in variables)
                Variables.Add(variable);
            OriginalRange = expression.OriginalRange;
        }

        /// <summary>
        /// The variables that will get assigned to
        /// </summary>
        public IList<IVariable> Variables
        {
            get;
        }

        /// <summary>
        /// The expression to assign to <see cref="Variables"/>
        /// </summary>
        public Expression<TInstruction> Expression
        {
            get => _expression;
            set => UpdateChildNotNull(ref _expression, value);
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren()
        {
            yield return Expression;
        }

        /// <inheritdoc />
        protected internal override void OnAttach(CompilationUnit<TInstruction> newRoot)
        {
            for (int index = 0; index < Variables.Count; index++)
                newRoot.RegisterVariableWrite(Variables[index], this);
            Expression.OnAttach(newRoot);
        }

        /// <inheritdoc />
        protected internal override void OnDetach(CompilationUnit<TInstruction> oldRoot)
        {
            for (int index = 0; index < Variables.Count; index++)
                oldRoot.UnregisterVariableWrite(Variables[index], this);
            Expression.OnDetach(oldRoot);
        }

        /// <inheritdoc />
        public override void Accept(IAstNodeVisitor<TInstruction> visitor) 
            => visitor.Visit(this);

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) 
            => visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) 
            => visitor.Visit(this, state);

        /// <summary>
        /// Modifies the current <see cref="AssignmentStatement{TInstruction}"/> to assign to <paramref name="variables"/>
        /// </summary>
        /// <param name="variables">The variables to assign the <see cref="Expression"/> to</param>
        /// <returns>The same <see cref="AssignmentStatement{TInstruction}"/> instance but with the new <paramref name="variables"/></returns>
        public AssignmentStatement<TInstruction> WithVariables(params IVariable[] variables) =>
            WithVariables(variables as IEnumerable<IVariable>);

        /// <summary>
        /// Modifies the current <see cref="AssignmentStatement{TInstruction}"/> to assign to <paramref name="variables"/>
        /// </summary>
        /// <param name="variables">The variables to assign the <see cref="Expression"/> to</param>
        /// <returns>The same <see cref="AssignmentStatement{TInstruction}"/> instance but with the new <paramref name="variables"/></returns>
        public AssignmentStatement<TInstruction> WithVariables(IEnumerable<IVariable> variables)
        {
            Variables.Clear();
            
            foreach (var variable in variables)
                Variables.Add(variable);

            return this;
        }

        /// <summary>
        /// Modifies the current <see cref="AssignmentStatement{TInstruction}"/> to assign the value of <paramref name="expression"/>
        /// </summary>
        /// <param name="expression">The <see cref="Expression{TInstruction}"/> to assign</param>
        /// <returns>The same <see cref="AssignmentStatement{TInstruction}"/> instance but with the new <paramref name="expression"/></returns>
        public AssignmentStatement<TInstruction> WithExpression(Expression<TInstruction> expression)
        {
            Expression = expression;
            return this;
        }
    }
}