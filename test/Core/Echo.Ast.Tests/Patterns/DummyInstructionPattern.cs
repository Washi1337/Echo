using System.Collections.Generic;
using Echo.Ast.Patterns;
using Echo.Platforms.DummyPlatform.Code;

namespace Echo.Ast.Tests.Patterns
{
    public class DummyInstructionPattern : Pattern<DummyInstruction>
    {
        public DummyInstructionPattern(DummyOpCode opCode)
        {
            OpCode = new LiteralPattern<DummyOpCode>(opCode);
        }
        
        public DummyInstructionPattern(Pattern<DummyOpCode> opCode)
        {
            OpCode = opCode;
        }
        
        public DummyInstructionPattern(DummyOpCode opCode, params Pattern<object>[] operands)
        {
            OpCode = new LiteralPattern<DummyOpCode>(opCode);
            foreach (var operand in operands)
                Operands.Add(operand);
        }

        public DummyInstructionPattern(Pattern<DummyOpCode> opCode, params Pattern<object>[] operands)
        {
            OpCode = opCode;
            foreach (var operand in operands)
                Operands.Add(operand);
        }

        public Pattern<DummyOpCode> OpCode
        {
            get;
            set;
        }

        public IList<Pattern<object>> Operands
        {
            get;
        } = new List<Pattern<object>>();
        
        protected override void MatchChildren(DummyInstruction input, MatchResult result)
        {
            OpCode.Match(input.OpCode, result);
            if (!result.IsSuccess)
                return;

            for (int i = 0; i < Operands.Count; i++)
            {
                Operands[i].Match(input.Operands[i], result);
                if (!result.IsSuccess)
                    return;
            }
        }

        public override string ToString()
        {
            return $"{OpCode} {string.Join(", ", Operands)}";
        }
    }
}