using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides a custom formatter for <see cref="CilInstruction"/>s.
    /// </summary>
    public class CilInstructionFormatter : IInstructionFormatter<CilInstruction>
    {
        /// <inheritdoc />
        public string Format(in CilInstruction instruction) => $"{instruction.OpCode.Mnemonic} {instruction.Operand}";
    }
}