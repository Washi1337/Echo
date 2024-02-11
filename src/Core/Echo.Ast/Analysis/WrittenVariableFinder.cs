using System.Buffers;
using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class WrittenVariableFinder<TInstruction> : AstNodeListener<TInstruction>
    {
        private readonly IArchitecture<TInstruction> _architecture;

        public WrittenVariableFinder(IArchitecture<TInstruction> architecture)
        {
            _architecture = architecture;
        }
        
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

        public override void ExitInstructionExpression(InstructionExpression<TInstruction> expression)
        {
            int count = _architecture.GetWrittenVariablesCount(expression.Instruction);
            if (count == 0)
                return;
            
            var variables = ArrayPool<IVariable>.Shared.Rent(count);
            
            int actualCount = _architecture.GetWrittenVariables(expression.Instruction, variables);
            for (int i = 0; i < actualCount; i++)
                Variables.Add(variables[i]);
            
            ArrayPool<IVariable>.Shared.Return(variables);
            
            base.ExitInstructionExpression(expression);
        }
    }
}