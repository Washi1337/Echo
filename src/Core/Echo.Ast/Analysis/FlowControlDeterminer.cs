using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class FlowControlDeterminer<TInstruction>
        : IAstNodeVisitor<TInstruction, object, InstructionFlowControl>
    {
        private readonly IArchitecture<TInstruction> _isa;
        
        internal FlowControlDeterminer(IArchitecture<TInstruction> isa) =>
            _isa = isa;

        public InstructionFlowControl Visit(AssignmentStatement<TInstruction> statement, object state) =>
            statement.Expression.Accept(this, state);

        public InstructionFlowControl Visit(ExpressionStatement<TInstruction> statement, object state) =>
            statement.Expression.Accept(this, state);

        public InstructionFlowControl Visit(PhiStatement<TInstruction> statement, object state) =>
            InstructionFlowControl.Fallthrough;

        public InstructionFlowControl Visit(InstructionExpression<TInstruction> expression, object state) =>
            _isa.GetFlowControl(expression.Instruction);

        public InstructionFlowControl Visit(VariableExpression<TInstruction> expression, object state) =>
            InstructionFlowControl.Fallthrough;
    }
}