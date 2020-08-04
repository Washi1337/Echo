using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Ast
{
    public class AstStatementFormatter<TInstruction> : IInstructionFormatter<AstStatementBase<TInstruction>>
    {
        private readonly IInstructionFormatter<TInstruction> _instructionFormatter;
        
        public AstStatementFormatter(IInstructionFormatter<TInstruction> instructionFormatter)
        {
            _instructionFormatter = instructionFormatter;
        }

        /// <inheritdoc />
        public string Format(in AstStatementBase<TInstruction> instruction) =>
            instruction.Format(_instructionFormatter);
    }
}