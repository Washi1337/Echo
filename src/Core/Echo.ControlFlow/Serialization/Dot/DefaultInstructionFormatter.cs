namespace Echo.ControlFlow.Serialization.Dot
{
    internal sealed class DefaultInstructionFormatter<TInstruction> : IInstructionFormatter<TInstruction>
    {
        internal static readonly DefaultInstructionFormatter<TInstruction> Instance
            = new DefaultInstructionFormatter<TInstruction>();
        
        private DefaultInstructionFormatter() { }
        
        public string Format(in TInstruction instruction) => instruction.ToString();
    }
}