using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class WrittenVariableFinder<TInstruction> : AstNodeListener<TInstruction>
    {
        internal HashSet<IVariable> Variables { get; } = new();

        public override void ExitAssignmentStatement(AssignmentStatement<TInstruction> statement)
        {
            base.ExitAssignmentStatement(statement);
            for (int i = 0; i < statement.Variables.Count; i++)
                Variables.Add(statement.Variables[i]);
        }

        public override void ExitPhiStatement(PhiStatement<TInstruction> phiStatement)
        {
            base.ExitPhiStatement(phiStatement);
            Variables.Add(phiStatement.Representative);
        }
    }
}