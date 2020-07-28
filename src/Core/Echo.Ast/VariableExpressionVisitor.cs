using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.Ast
{
    internal sealed class VariableExpressionVisitor<TInstruction>
    {
        private readonly HashSet<IVariable> _variables = new HashSet<IVariable>();

        internal int Count => _variables.Count;

        internal IVariable[] Variables => _variables.ToArray();

        internal void Visit(AstInstructionExpression<TInstruction> instructionExpression)
        {
            foreach (var parameter in instructionExpression.Parameters)
                parameter.Accept(this);
        }

        internal void Visit(AstVariableExpression<TInstruction> variableExpression)
        {
            _variables.Add(variableExpression.Variable);
        }
    }
}