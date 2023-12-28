namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Provides a default implementation for <see cref="IInstructionFormatter{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction to create a formatter of.</typeparam>
    public sealed class DefaultInstructionFormatter<TInstruction> : IInstructionFormatter<TInstruction>
    {
        /// <summary>
        /// Gets a singleton instance of the <see cref="DefaultInstructionFormatter{TInstruction}"/> class.
        /// </summary>
        public static DefaultInstructionFormatter<TInstruction> Instance { get; } = new();

        /// <inheritdoc />
        public string Format(in TInstruction instruction) => instruction.ToString();
    }
}