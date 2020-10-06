namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Allows the user to format instructions in the <see cref="ControlFlowNodeAdorner{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the nodes contain.</typeparam>
    public interface IInstructionFormatter<TInstruction>
    {
        /// <summary>
        /// Formats a given <typeparamref name="TInstruction"/>.
        /// </summary>
        /// <param name="instruction">The <typeparamref name="TInstruction"/> to format.</param>
        /// <returns>The formatted <paramref name="instruction"/>.</returns>
        string Format(in TInstruction instruction);
    }
}