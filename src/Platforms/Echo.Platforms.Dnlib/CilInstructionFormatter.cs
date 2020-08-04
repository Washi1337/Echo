using dnlib.DotNet.Emit;
using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Provides a custom formatter for <see cref="Instruction"/>s.
    /// </summary>
    public class CilInstructionFormatter : IInstructionFormatter<Instruction>
    {
        /// <inheritdoc />
        public string Format(in Instruction instruction) => $"{instruction.OpCode.Name} {instruction.Operand}";
    }
}