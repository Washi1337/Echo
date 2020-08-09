using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.Ast.Helpers
{
    internal sealed class WrittenVariableFinderVisitor<TInstruction> : INodeVisitor<TInstruction, object>
    {
        private readonly HashSet<IVariable> _variables = new HashSet<IVariable>();

        internal int Count => _variables.Count;

        internal IVariable[] Variables => _variables.ToArray();
        
        public void Visit(AssignmentStatement<TInstruction> assignmentStatement, object state)
        {
            foreach (var target in assignmentStatement.Variables)
                _variables.Add(target);
        }

        public void Visit(ExpressionStatement<TInstruction> expressionStatement, object state) { }
        
        public void Visit(PhiStatement<TInstruction> phiStatement, object state) =>
            _variables.Add(phiStatement.Target);

        public void Visit(InstructionExpression<TInstruction> instructionExpression, object state) { }

        public void Visit(VariableExpression<TInstruction> variableExpression, object state) { }
    }
}