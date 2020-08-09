using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.Ast.Helpers
{
    internal sealed class ReadVariableFinderVisitor<TInstruction> : INodeVisitor<TInstruction, object>
    {
        private readonly HashSet<IVariable> _variables = new HashSet<IVariable>();

        internal int Count => _variables.Count;

        internal IVariable[] Variables => _variables.ToArray();

        public void Visit(AssignmentStatement<TInstruction> assignmentStatement, object state) =>
            assignmentStatement.Expression.Accept(this, state);

        public void Visit(ExpressionStatement<TInstruction> expressionStatement, object state) =>
            expressionStatement.Expression.Accept(this, state);

        public void Visit(PhiStatement<TInstruction> phiStatement, object state)
        {
            foreach (var source in phiStatement.Sources)
                source.Accept(this, state);
        }

        public void Visit(InstructionExpression<TInstruction> instructionExpression, object state)
        {
            foreach (var parameter in instructionExpression.Arguments)
                parameter.Accept(this, state);
        }

        public void Visit(VariableExpression<TInstruction> variableExpression, object state) =>
            _variables.Add(variableExpression.Variable);
    }
}