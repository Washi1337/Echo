using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class FlowControlDeterminer<TInstruction>
        : IAstNodeVisitor<TInstruction, object, InstructionFlowControl>
    {
        private readonly IInstructionSetArchitecture<TInstruction> _isa;
        
        internal FlowControlDeterminer(IInstructionSetArchitecture<TInstruction> isa) =>
            _isa = isa;

        public InstructionFlowControl Visit(AssignmentStatement<TInstruction> assignmentStatement, object state) =>
            assignmentStatement.Expression.Accept(this, state);

        public InstructionFlowControl Visit(ExpressionStatement<TInstruction> expressionStatement, object state) =>
            expressionStatement.Expression.Accept(this, state);

        public InstructionFlowControl Visit(PhiStatement<TInstruction> phiStatement, object state) =>
            InstructionFlowControl.Fallthrough;

        public InstructionFlowControl Visit(InstructionExpression<TInstruction> instructionExpression, object state) =>
            _isa.GetFlowControl(instructionExpression.Instruction);

        public InstructionFlowControl Visit(VariableExpression<TInstruction> variableExpression, object state) =>
            InstructionFlowControl.Fallthrough;
    }
}