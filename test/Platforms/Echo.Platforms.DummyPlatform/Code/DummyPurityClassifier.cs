using System;
using Echo.Code;

namespace Echo.Platforms.DummyPlatform.Code;

public class DummyPurityClassifier : IPurityClassifier<DummyInstruction>
{
    public static DummyPurityClassifier Instance
    {
        get;
    } = new();
    
    public Trilean IsPure(in DummyInstruction instruction)
    {
        return instruction.OpCode switch
        {
            DummyOpCode.Op => Trilean.Unknown,
            DummyOpCode.Push => true,
            DummyOpCode.Pop => true,
            DummyOpCode.Get => true,
            DummyOpCode.Set => false,
            DummyOpCode.Jmp => true,
            DummyOpCode.JmpCond => true,
            DummyOpCode.Ret => true,
            DummyOpCode.Switch => true,
            DummyOpCode.PushOffset => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}