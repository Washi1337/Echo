using System.Buffers;
using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class ReadVariableFinder<TInstruction> : AstNodeListener<TInstruction>
        where TInstruction : notnull
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
            _architecture.GetReadVariables(expression.Instruction, Variables);
            base.ExitInstructionExpression(expression);
        }
    }
}