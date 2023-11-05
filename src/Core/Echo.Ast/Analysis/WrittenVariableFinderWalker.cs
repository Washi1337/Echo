using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class WrittenVariableFinderWalker<TInstruction> : AstNodeWalker<TInstruction>
    {
        internal int Count => Variables.Count;
        
        internal HashSet<IVariable> Variables
        {
            get;
        } = new HashSet<IVariable>();

        protected override void ExitAssignmentStatement(AssignmentStatement<TInstruction> assignmentStatement)
        {
            foreach (var target in assignmentStatement.Variables)
                Variables.Add(target);
        }

        protected override void ExitPhiStatement(PhiStatement<TInstruction> phiStatement) =>
            Variables.Add(phiStatement.Representative);
    }
}