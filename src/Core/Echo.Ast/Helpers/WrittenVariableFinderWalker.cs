using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.Ast.Helpers
{
    internal sealed class WrittenVariableFinderWalker<TInstruction> : AstNodeWalkerBase<TInstruction, object>
    {
        private readonly HashSet<IVariable> _variables = new HashSet<IVariable>();

        internal int Count => _variables.Count;

        internal IVariable[] Variables => _variables.ToArray();

        protected override void ExitAssignmentStatement(
            AssignmentStatement<TInstruction> assignmentStatement, object state)
        {
            foreach (var target in assignmentStatement.Variables)
                _variables.Add(target);
        }

        protected override void ExitPhiStatement(PhiStatement<TInstruction> phiStatement, object state) =>
            _variables.Add(phiStatement.Target);
    }
}