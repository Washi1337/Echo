using System.Collections.Generic;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Represents a single handler block in an <see cref="ExceptionHandlerBlock{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that this block contains.</typeparam>
    public class HandlerBlock<TInstruction> : IBlock<TInstruction>
    {
        /// <summary>
        /// Gets or sets the prologue block that gets executed before the main handler block (if available).
        /// </summary>
        public ScopeBlock<TInstruction> Prologue
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
        } = new ScopeBlock<TInstruction>();

        /// <summary>
        /// Gets or sets the epilogue block that gets executed after the main handler block (if available).
        /// </summary>
        public ScopeBlock<TInstruction> Epilogue
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
        public void AcceptVisitor(IBlockVisitor<TInstruction> visitor) => visitor.VisitHandlerBlock(this);
    }
}