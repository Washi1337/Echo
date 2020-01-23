using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IInstructionTraversalResult{TInstruction}"/> interface,
    /// using a dictionary and a set to store the instructions and block header offsets. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that were traversed.</typeparam>
    public class InstructionTraversalResult<TInstruction> : IInstructionTraversalResult<TInstruction>
    {
        /// <summary>
        /// Gets a collection of instructions and their metadata, grouped by their offsets. 
        /// </summary>
        public IDictionary<long, InstructionInfo<TInstruction>> Instructions
        {
            get;
        } = new Dictionary<long, InstructionInfo<TInstruction>>();

        /// <summary>
        /// Gets a collection of recorded block headers.
        /// </summary>
        public ISet<long> BlockHeaders
        {
            get;
        } = new HashSet<long>();

        /// <inheritdoc />
        public bool IsBlockHeader(long offset) => BlockHeaders.Contains(offset);

        /// <inheritdoc />
        public bool ContainsInstruction(long offset) => Instructions.ContainsKey(offset);

        /// <inheritdoc />
        public InstructionInfo<TInstruction> GetInstruction(long offset) => Instructions[offset];

        /// <inheritdoc />
        public IEnumerable<InstructionInfo<TInstruction>> GetAllInstructions() => Instructions
            .OrderBy(e => e.Key)
            .Select(e => e.Value);
    }
}