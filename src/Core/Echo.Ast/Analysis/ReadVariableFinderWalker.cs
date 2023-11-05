using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class ReadVariableFinderWalker<TInstruction> : AstNodeWalker<TInstruction>
    {
        internal int Count => Variables.Count;

        internal HashSet<IVariable> Variables
        {
            get;
        } = new HashSet<IVariable>();

        protected override void VisitVariableExpression(VariableExpression<TInstruction> variableExpression) =>
            Variables.Add(variableExpression.Variable);
    }
}