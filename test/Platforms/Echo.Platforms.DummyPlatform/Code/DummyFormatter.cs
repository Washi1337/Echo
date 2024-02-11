using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Platforms.DummyPlatform.Code;

public class DummyFormatter : IInstructionFormatter<DummyInstruction>
{
    public bool IncludeOffset
    {
        get;
        set;
    } = true;
    
    public string Format(in DummyInstruction instruction)
    {
        return IncludeOffset 
            ? $"Label_{instruction.Offset:X4}: {instruction.Mnemonic} {string.Join(", ", instruction.Operands)}"
            : $"{instruction.Mnemonic} {string.Join(", ", instruction.Operands)}";
    }
}