using Echo.ControlFlow.Serialization.Dot;
using Iced.Intel;

namespace Echo.Platforms.Iced
{
    /// <summary>
    /// Provides a custom formatter for <see cref="Instruction"/>s.
    /// </summary>
    public class CilInstructionFormatter : IInstructionFormatter<Instruction>
    {
        /// <inheritdoc />
        public string Format(in Instruction instruction) => $"{instruction}";
    }
}