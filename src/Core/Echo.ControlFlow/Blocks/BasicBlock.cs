using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Blocks
{
    /// <summary>
    /// Represents one basic block in a control flow graph, consisting of a list of instructions (or statements).
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that the basic block contains.</typeparam>
    public class BasicBlock<TInstruction> : IBlock<TInstruction>
    {
        /// <summary>
        /// Creates a new, empty basic block.
        /// </summary>
        public BasicBlock()
            : this(Enumerable.Empty<TInstruction>())
        {    
        }

        /// <summary>
        /// Creates a new, empty basic block, with the provided offset.
        /// </summary>
        /// <param name="offset">The offset to assign to the basic block.</param>
        public BasicBlock(long offset)
            : this(offset, Enumerable.Empty<TInstruction>())
        {
        }
        
        /// <summary>
        /// Creates a new basic block with the provided instructions. 
        /// </summary>
        /// <param name="instructions">The instructions to add to the basic block.</param>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="instructions"/> is <c>null</c>.</exception>
        public BasicBlock(IEnumerable<TInstruction> instructions)
            : this(-1, instructions)
        {
        }

        /// <summary>
        /// Creates a new basic block with the provided offset and list of instructions. 
        /// </summary>
        /// <param name="offset">The offset to assign to the basic block.</param>
        /// <param name="instructions">The instructions to add to the basic block.</param>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="instructions"/> is <c>null</c>.</exception>
        public BasicBlock(long offset, IEnumerable<TInstruction> instructions)
        {
            if (instructions == null)
                throw new ArgumentNullException(nameof(instructions));
            Offset = offset;
            Instructions = new List<TInstruction>(instructions);   
        }

        /// <summary>
        /// Gets or sets the offset (or identifier) of this basic block.
        /// </summary>
        public long Offset
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets a collection of isntructions that are executed in sequence when this basic block is executed.
        /// </summary>
        public IList<TInstruction> Instructions
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the basic block contains any instruction.
        /// </summary>
        public bool IsEmpty => Instructions.Count == 0;

        /// <summary>
        /// Gets the first instruction that is evaluated when this basic block is executed.
        /// </summary>
        public TInstruction Header => !IsEmpty ? Instructions[0] : default;

        /// <summary>
        /// Gets the last instruction that is evaluated when this basic block is executed.
        /// </summary>
        public TInstruction Footer => !IsEmpty ? Instructions[Instructions.Count - 1] : default;
        
        /// <inheritdoc />
        public override string ToString()
        {
            string newLine = Environment.NewLine;
            return "{" + newLine + string.Join(newLine, Instructions) + newLine + "}";
        }

        IEnumerable<BasicBlock<TInstruction>> IBlock<TInstruction>.GetAllBlocks()
        {
            return new[] {this};
        }
    }
}