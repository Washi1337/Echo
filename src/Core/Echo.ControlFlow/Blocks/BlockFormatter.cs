using System;
using System.Text;

namespace Echo.ControlFlow.Blocks
{
    public class BlockFormatter<TInstruction> : IBlockListener<TInstruction>
    {
        private const string DefaultIndentationString = "    ";

        public static string Format(IBlock<TInstruction> block)
        {
            var formatter = new BlockFormatter<TInstruction>(DefaultIndentationString);
            var walker = new BlockWalker<TInstruction>(formatter);
            walker.Walk(block);
            return formatter.GetOutput();
        } 
            
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly string _indentationString;
        private int _indentationLevel;

        public BlockFormatter(string indentationString)
        {
            _indentationString = indentationString ?? throw new ArgumentNullException(nameof(indentationString));
        }

        public string GetOutput() => _builder.ToString();

        private void AppendIndentationString()
        {
            for (int i = 0; i < _indentationLevel; i++)
                _builder.Append(_indentationString);
        }

        public void VisitBasicBlock(BasicBlock<TInstruction> block)
        { 
            AppendIndentationString();
            _builder.AppendFormat("Block_{0:X8}:", block.Offset);
            _builder.AppendLine();
            
            foreach (var instruction in block.Instructions)
            {
                AppendIndentationString();
                _builder.Append(instruction);
                _builder.AppendLine();
            }
        }

        public void EnterScopeBlock(ScopeBlock<TInstruction> block)
        {
            AppendIndentationString();
            _builder.Append('{');
            _builder.AppendLine();
            
            _indentationLevel++;
        }

        public void ExitScopeBlock(ScopeBlock<TInstruction> block)
        {
            _indentationLevel--;
            
            AppendIndentationString();
            _builder.Append('}');
            _builder.AppendLine();
        }
    }
}