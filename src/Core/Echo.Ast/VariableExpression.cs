using System;
using System.Collections.Generic;
using Echo.Code;
using Echo.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a variable expression in the AST
    /// </summary>
    public sealed class VariableExpression<TInstruction> : Expression<TInstruction>
        where TInstruction : notnull
    {
        private IVariable _variable = null!;

        /// <summary>
        /// Creates a new variable expression
        /// </summary>
        /// <param name="variable">The variable</param>
        public VariableExpression(IVariable variable)
        {
            Variable = variable;
        }

        /// <summary>
        /// The variable that is represented by the AST node
        /// </summary>
        public IVariable Variable
        {
            get => _variable;
            internal set
            {
                if (_variable != value)
                { 
                    // Force detach/attach to update variable xrefs.
                    var root = GetParentCompilationUnit();
                    if (root is not null)
                        OnDetach(root);
                    
                    _variable = value;
                    
                    if (root is not null)
                        OnAttach(root);
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren() => Array.Empty<TreeNodeBase>();

        /// <inheritdoc />
        protected internal override void OnAttach(CompilationUnit<TInstruction> newRoot)
        {
            newRoot.RegisterVariableUse(this);
        }

        /// <inheritdoc />
        protected internal override void OnDetach(CompilationUnit<TInstruction> oldRoot)
        {
            oldRoot.UnregisterVariableUse(this);
        }

        /// <inheritdoc />
        public override void Accept(IAstNodeVisitor<TInstruction> visitor) 
            => visitor.Visit(this);

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
            visitor.Visit(this, state);

        /// <summary>
        /// Modifies the current <see cref="VariableExpression{TInstruction}"/> to target <paramref name="variable"/>
        /// </summary>
        /// <param name="variable">The new <see cref="IVariable"/> to target</param>
        /// <returns>The same <see cref="VariableExpression{TInstruction}"/> instance but with the new <paramref name="variable"/></returns>
        public VariableExpression<TInstruction> WithVariable(IVariable variable)
        {
            Variable = variable;
            return this;
        }
    }
}