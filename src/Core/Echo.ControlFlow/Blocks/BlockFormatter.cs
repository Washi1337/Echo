using System;
using System.Text;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides a mechanism for formatting a tree of blocks into an indented string representation.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the block.</typeparam>
    public class BlockFormatter<TInstruction> : IBlockListener<TInstruction>
    {
        private const string DefaultIndentationString = "    ";

        /// <summary>
        /// Formats a block into an indented string representation.
        /// </summary>
        /// <param name="block">The block to format.</param>
        /// <returns>The indented string.</returns>
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

        /// <summary>
        /// Creates a new block formatter.
        /// </summary>
        /// <param name="indentationString">The string to use when indenting a block.</param>
        public BlockFormatter(string indentationString)
        {
            _indentationString = indentationString ?? throw new ArgumentNullException(nameof(indentationString));
        }

        /// <summary>
        /// Obtains the raw string output.
        /// </summary>
        public string GetOutput() => _builder.ToString();

        private void AppendIndentationString()
        {
            for (int i = 0; i < _indentationLevel; i++)
                _builder.Append(_indentationString);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void EnterScopeBlock(ScopeBlock<TInstruction> block)
        {
            AppendIndentationString();
            _builder.Append('{');
            _builder.AppendLine();
            
            _indentationLevel++;
        }

        /// <inheritdoc />
        public void ExitScopeBlock(ScopeBlock<TInstruction> block)
        {
            _indentationLevel--;
            
            AppendIndentationString();
            _builder.Append('}');
            _builder.AppendLine();
        }
    }
}