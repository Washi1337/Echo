using System.Buffers;
using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class ReadVariableFinder<TInstruction> : AstNodeListener<TInstruction>
    {
        private readonly IArchitecture<TInstruction> _architecture;

        public ReadVariableFinder(IArchitecture<TInstruction> architecture)
        {
            _architecture = architecture;
        }

        internal HashSet<IVariable> Variables { get; } = new();

        public override void ExitVariableExpression(VariableExpression<TInstruction> expression)
        {
            base.ExitVariableExpression(expression);
            Variables.Add(expression.Variable);
        }

        public override void ExitInstructionExpression(InstructionExpression<TInstruction> expression)
        {
            int count = _architecture.GetReadVariablesCount(expression.Instruction);
            if (count == 0)
                return;
            
            var variables = ArrayPool<IVariable>.Shared.Rent(count);
            
            int actualCount = _architecture.GetReadVariables(expression.Instruction, variables);
            for (int i = 0; i < actualCount; i++)
                Variables.Add(variables[i]);
            
            ArrayPool<IVariable>.Shared.Return(variables);
            
            base.ExitInstructionExpression(expression);
        }
    }
}