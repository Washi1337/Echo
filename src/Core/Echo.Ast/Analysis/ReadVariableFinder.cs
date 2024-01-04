using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class ReadVariableFinder<TInstruction> : AstNodeListener<TInstruction>
    {
        internal HashSet<IVariable> Variables { get; } = new();

        public override void ExitVariableExpression(VariableExpression<TInstruction> expression)
        {
            base.ExitVariableExpression(expression);
            Variables.Add(expression.Variable);
        }
    }
}