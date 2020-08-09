using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Ast
{
    /// <summary>
    /// Formats the Ast so it looks nicer in <see cref="ControlFlowNodeAdorner{TInstruction}"/>
    /// </summary>
    public class StatementFormatter<TInstruction> : IInstructionFormatter<StatementBase<TInstruction>>
    {
        private readonly IInstructionFormatter<TInstruction> _instructionFormatter;
        
        /// <summary>
        /// Creates a new instance of <see cref="StatementFormatter{TInstruction}"/> with the specified
        /// <see cref="IInstructionFormatter{TInstruction}"/>
        /// </summary>
        /// <param name="instructionFormatter">The <see cref="IInstructionFormatter{TInstruction}"/> used to
        /// format <see cref="InstructionExpression{TInstruction}"/>s with</param>
        public StatementFormatter(IInstructionFormatter<TInstruction> instructionFormatter) =>
            _instructionFormatter = instructionFormatter;

        /// <inheritdoc />
        public string Format(in StatementBase<TInstruction> instruction) =>
            instruction.Format(_instructionFormatter);
    }
}