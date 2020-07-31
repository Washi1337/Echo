using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Ast
{
    public sealed class AstPhiStatement<TInstruction> : AstStatementBase<TInstruction>
    {
        public AstPhiStatement(long id, IEnumerable<AstVariableExpression<TInstruction>> sources, IVariable target) : base(id)
        {
            Sources = sources;
            Target = target;
        }

        public AstVariableExpression<TInstruction> Sources
        {
            get;
        }

        public IVariable Target
        {
            get;
        }

        /// <inheritdoc />
        public override IEnumerable<AstNodeBase<TInstruction>> GetChildren()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}