namespace Echo.ControlFlow.Serialization.Dot
{
    internal sealed class DefaultInstructionFormatter<TInstruction> : IInstructionFormatter<TInstruction>
    {
        public string Format(in TInstruction instruction)
        {
            return instruction.ToString();
        }
    }
}