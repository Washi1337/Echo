using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides a custom formatter for <see cref="CilInstruction"/>s.
    /// </summary>
    public class CilInstructionFormatterAdapter : CilInstructionFormatter, IInstructionFormatter<CilInstruction>
    {
        /// <inheritdoc />
        public string Format(in CilInstruction instruction)
        {
            string minimal = $"{FormatOpCode(instruction.OpCode)}";
            return instruction.Operand is null
                ? minimal
                : $"{minimal} {FormatOperand(instruction.OpCode.OperandType, instruction.Operand)}";
        }
    }
}