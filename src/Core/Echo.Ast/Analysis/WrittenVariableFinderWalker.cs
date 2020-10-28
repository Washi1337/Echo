using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class WrittenVariableFinderWalker<TInstruction> : AstNodeWalkerBase<TInstruction>
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
            Variables.Add(phiStatement.Target);
    }
}