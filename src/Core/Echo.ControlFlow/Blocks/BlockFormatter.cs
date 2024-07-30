using System;
using System.Text;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Provides a mechanism for formatting a tree of blocks into an indented string representation.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the block.</typeparam>
    public class BlockFormatter<TInstruction> : IBlockListener<TInstruction>
        where TInstruction : notnull
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
        public void EnterScopeBlock(ScopeBlock<TInstruction> block) => OpenScope();

        /// <inheritdoc />
        public void ExitScopeBlock(ScopeBlock<TInstruction> block) => CloseScope();

        /// <inheritdoc />
        public void EnterExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block) => OpenScope();

        /// <inheritdoc />
        public void ExitExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block) => CloseScope();

        /// <inheritdoc />
        public void EnterProtectedBlock(ExceptionHandlerBlock<TInstruction> block)
        {
            AppendIndentationString();
            _builder.AppendLine("try:");
        }

        /// <inheritdoc />
        public void ExitProtectedBlock(ExceptionHandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public void EnterHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex)
        {
            AppendIndentationString();
            _builder.AppendLine($"handler{handlerIndex}:");
            
            OpenScope();
        }

        /// <inheritdoc />
        public void ExitHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex) => CloseScope();

        /// <inheritdoc />
        public void EnterPrologueBlock(HandlerBlock<TInstruction> block)
        {
            AppendIndentationString();
            _builder.AppendLine("prologue:");
        }

        /// <inheritdoc />
        public void ExitPrologueBlock(HandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public void EnterEpilogueBlock(HandlerBlock<TInstruction> block)
        {
            AppendIndentationString();
            _builder.AppendLine("epilogue:");
        }

        /// <inheritdoc />
        public void ExitEpilogueBlock(HandlerBlock<TInstruction> block)
        {
        }

        /// <inheritdoc />
        public void EnterHandlerContents(HandlerBlock<TInstruction> block)
        {
            AppendIndentationString();
            _builder.AppendLine("code:");
        }

        /// <inheritdoc />
        public void ExitHandlerContents(HandlerBlock<TInstruction> block)
        {
        }
        
        private void OpenScope()
        {
            AppendIndentationString();
            _builder.Append('{');
            _builder.AppendLine();

            _indentationLevel++;
        }

        private void CloseScope()
        {
            _indentationLevel--;

            AppendIndentationString();
            _builder.Append('}');
            _builder.AppendLine();
        }
    }
}