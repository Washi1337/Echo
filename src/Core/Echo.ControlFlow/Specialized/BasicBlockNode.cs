using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Specialized.Blocks;

namespace Echo.ControlFlow.Specialized
{
    /// <summary>
    /// Represents a node that contains one basic block.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the basic block.</typeparam>
    public class BasicBlockNode<TInstruction> : Node<BasicBlock<TInstruction>>
    {
        /// <summary>
        /// Creates a basic block node with an empty basic block inside of it.  
        /// </summary>
        public BasicBlockNode() 
            : this(Enumerable.Empty<TInstruction>())
        {
        }

        /// <summary>
        /// Creates a basic block node with the provided instructions.
        /// </summary>
        /// <param name="instructions">The instructions to store.</param>
        public BasicBlockNode(IEnumerable<TInstruction> instructions)
            : this(new BasicBlock<TInstruction>(instructions))
        {
        }


        /// <summary>
        /// Creates a basic block node with the provided instructions.
        /// </summary>
        /// <param name="offset">The offset (i.e. identifier) of the new node.</param>
        /// <param name="instructions">The instructions to store.</param>
        public BasicBlockNode(long offset, IEnumerable<TInstruction> instructions)
            : this(new BasicBlock<TInstruction>(offset, instructions))
        {
        }

        /// <summary>
        /// Creates a basic block node with the provided contents.
        /// </summary>
        /// <param name="basicBlock">The data to store in the basic block.</param>
        public BasicBlockNode(BasicBlock<TInstruction> basicBlock)
            : base(basicBlock)
        {
        }
    }
}