using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.Ast.Helpers
{
    internal sealed class ReadVariableFinderWalker<TInstruction> : AstNodeWalkerBase<TInstruction>
    {
        private readonly HashSet<IVariable> _variables = new HashSet<IVariable>();

        internal int Count => _variables.Count;

        internal IVariable[] Variables => _variables.ToArray();

        protected override void VisitVariableExpression(VariableExpression<TInstruction> variableExpression) =>
            _variables.Add(variableExpression.Variable);
    }
}