using System;
using Echo.Code;

namespace Echo.Ast.Analysis
{
    internal sealed class FlowControlDeterminer<TInstruction>
        : IAstNodeVisitor<TInstruction, object?, InstructionFlowControl>
    {
        private readonly IArchitecture<TInstruction> _isa;
        
        internal FlowControlDeterminer(IArchitecture<TInstruction> isa) =>
            _isa = isa;

        public InstructionFlowControl Visit(CompilationUnit<TInstruction> unit, object? state) =>
            throw new NotSupportedException();

        public InstructionFlowControl Visit(AssignmentStatement<TInstruction> statement, object? state) =>
            statement.Expression.Accept(this, state);

        public InstructionFlowControl Visit(ExpressionStatement<TInstruction> statement, object? state) =>
            statement.Expression.Accept(this, state);

        public InstructionFlowControl Visit(PhiStatement<TInstruction> statement, object? state) =>
            InstructionFlowControl.Fallthrough;

        public InstructionFlowControl Visit(BlockStatement<TInstruction> statement, object? state)
        {
            return statement.Statements.Count > 0
                ? statement.Statements[statement.Statements.Count - 1].Accept(this, state)
                : InstructionFlowControl.Fallthrough;
        }

        public InstructionFlowControl Visit(ExceptionHandlerStatement<TInstruction> statement, object? state)
        {
            return statement.ProtectedBlock.Accept(this, state);
        }

        public InstructionFlowControl Visit(HandlerClause<TInstruction> clause, object? state)
        {
            throw new NotSupportedException();
        }

        public InstructionFlowControl Visit(InstructionExpression<TInstruction> expression, object? state) =>
            _isa.GetFlowControl(expression.Instruction);

        public InstructionFlowControl Visit(VariableExpression<TInstruction> expression, object? state) =>
            InstructionFlowControl.Fallthrough;
    }
}