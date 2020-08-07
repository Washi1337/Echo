using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Ast
{
    /// <summary>
    /// Formats the Ast so it looks nicer in <see cref="ControlFlowNodeAdorner{TInstruction}"/>
    /// </summary>
    public class AstStatementFormatter<TInstruction> : IInstructionFormatter<AstStatementBase<TInstruction>>
    {
        private readonly IInstructionFormatter<TInstruction> _instructionFormatter;
        
        /// <summary>
        /// Creates a new instance of <see cref="AstStatementFormatter{TInstruction}"/> with the specified
        /// <see cref="IInstructionFormatter{TInstruction}"/>
        /// </summary>
        /// <param name="instructionFormatter">The <see cref="IInstructionFormatter{TInstruction}"/> used to
        /// format <see cref="AstInstructionExpression{TInstruction}"/>s with</param>
        public AstStatementFormatter(IInstructionFormatter<TInstruction> instructionFormatter) =>
            _instructionFormatter = instructionFormatter;

        /// <inheritdoc />
        public string Format(in AstStatementBase<TInstruction> instruction) =>
            instruction.Format(_instructionFormatter);
    }
}