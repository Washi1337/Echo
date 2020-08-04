namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Provides a default implementation for <see cref="IInstructionFormatter{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction to create a formatter of.</typeparam>
    public sealed class DefaultInstructionFormatter<TInstruction> : IInstructionFormatter<TInstruction>
    {
        /// <inheritdoc />
        public string Format(in TInstruction instruction) => instruction.ToString();
    }
}