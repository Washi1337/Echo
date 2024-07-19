using System.Collections.Generic;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Represents a single handler block in an <see cref="ExceptionHandlerBlock{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that this block contains.</typeparam>
    public class HandlerBlock<TInstruction> : IBlock<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Gets or sets the prologue block that gets executed before the main handler block (if available).
        /// </summary>
        public ScopeBlock<TInstruction>? Prologue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the main scope block that forms the code for the handler block.
        /// </summary>
        public ScopeBlock<TInstruction> Contents
        {
            get;
        } = new();

        /// <summary>
        /// Gets or sets the epilogue block that gets executed after the main handler block (if available).
        /// </summary>
        public ScopeBlock<TInstruction>? Epilogue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a user-defined tag that is assigned to this block. 
        /// </summary>
        public object? Tag
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        public IEnumerable<BasicBlock<TInstruction>> GetAllBlocks()
        {
            if (Prologue != null)
            {
                foreach (var block in Prologue.GetAllBlocks())
                    yield return block;
            }

            foreach (var block in Contents.GetAllBlocks())
                yield return block;
            
            if (Epilogue != null)
            {
                foreach (var block in Epilogue.GetAllBlocks())
                    yield return block;
            }
        }

        /// <inheritdoc />
        public BasicBlock<TInstruction>? GetFirstBlock() =>
            Prologue?.GetFirstBlock()
            ?? Contents.GetFirstBlock()
            ?? Epilogue?.GetFirstBlock();

        /// <inheritdoc />
        public BasicBlock<TInstruction>? GetLastBlock() =>
            Epilogue?.GetLastBlock()
            ?? Contents.GetLastBlock()
            ?? Prologue?.GetLastBlock();

        /// <inheritdoc />
        public void AcceptVisitor(IBlockVisitor<TInstruction> visitor) => visitor.VisitHandlerBlock(this);

        /// <inheritdoc />
        public TResult AcceptVisitor<TState, TResult>(IBlockVisitor<TInstruction, TState, TResult> visitor, TState state) 
            => visitor.VisitHandlerBlock(this, state);
    }
}