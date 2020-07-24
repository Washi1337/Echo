using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Provides members for describing a traversal of a collection of instructions.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that were traversed.</typeparam>
    public interface IInstructionTraversalResult<TInstruction> : IStaticInstructionProvider<TInstruction>
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
        /// Obtains all instruction records that were collected during the traversal. 
        /// </summary>
        /// <returns>The instructions and their metadata.</returns>
        IEnumerable<TInstruction> GetAllInstructions();

        int GetSuccessorCount(long offset);

        int GetSuccessors(long offset, Span<SuccessorInfo> successorsBuffer);
    }
}