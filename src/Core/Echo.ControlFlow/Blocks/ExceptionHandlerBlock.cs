using System.Collections.Generic;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Represents a block of region that is protected by a set of exception handler blocks. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the blocks.</typeparam>
    public class ExceptionHandlerBlock<TInstruction> : IBlock<TInstruction>
    {
        /// <summary>
        /// Gets the protected block.
        /// </summary>
        public ScopeBlock<TInstruction> ProtectedBlock
        {
            get;
        } = new ScopeBlock<TInstruction>();

        /// <summary>
        /// Gets the prologue block.
        /// </summary>
        public ScopeBlock<TInstruction> PrologueBlock
        {
            get;
        } = new ScopeBlock<TInstruction>();

        /// <summary>
        /// Gets a collection of handler blocks.
        /// </summary>
        public IList<ScopeBlock<TInstruction>> HandlerBlocks
        {
            get;
        } = new List<ScopeBlock<TInstruction>>();

        /// <summary>
        /// Gets the epilogue block.
        /// </summary>
        public ScopeBlock<TInstruction> EpilogueBlock
        {
            get;
        } = new ScopeBlock<TInstruction>();

        /// <inheritdoc />
        public IEnumerable<BasicBlock<TInstruction>> GetAllBlocks()
        {
            foreach (var block in ProtectedBlock.GetAllBlocks())
                yield return block;
                
            foreach (var block in PrologueBlock.GetAllBlocks())
                yield return block;

            foreach (var handler in HandlerBlocks)
            {
                foreach (var block in handler.GetAllBlocks())
                    yield return block;
            }
                
            foreach (var block in EpilogueBlock.GetAllBlocks())
                yield return block;
        }

        /// <inheritdoc />
        public void AcceptVisitor(IBlockVisitor<TInstruction> visitor) => visitor.VisitExceptionHandlerBlock(this);

        /// <inheritdoc />
        public override string ToString() => BlockFormatter<TInstruction>.Format(this);
    }
}