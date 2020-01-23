using System.Collections.Generic;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Provides members for describing a traversal of a collection of instructions.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that were traversed.</typeparam>
    public interface IInstructionTraversalResult<TInstruction>
    {
        /// <summary>
        /// Determines whether an offset was marked as a block header during the traversal.
        /// </summary>
        /// <param name="offset">The offset to check.</param>
        /// <returns><c>true</c> if the offset was a block header, <c>false</c> otherwise.</returns>
        bool IsBlockHeader(long offset);

        /// <summary>
        /// Determines whether an offset was traversed and interpreted as an instruction.
        /// </summary>
        /// <param name="offset">The offset to check.</param>
        /// <returns><c>true</c> if the offset was traversed, <c>false</c> otherwise.</returns>
        bool ContainsInstruction(long offset);

        /// <summary>
        /// Obtains the information about an instruction by its offset that was collected during the traversal.
        /// </summary>
        /// <param name="offset">The offset of the instruction.</param>
        /// <returns>The collected instruction information.</returns>
        InstructionInfo<TInstruction> GetInstruction(long offset);

        /// <summary>
        /// Obtains all instruction records that were collected during the traversal. 
        /// </summary>
        /// <returns>The instructions and their metadata.</returns>
        IEnumerable<InstructionInfo<TInstruction>> GetAllInstructions();
    }
}